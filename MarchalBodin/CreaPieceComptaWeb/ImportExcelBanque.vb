Imports Objets100cLib
Imports Excel = Microsoft.Office.Interop.Excel
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports MBCore
Imports MBCore.Model
Imports System.ComponentModel

Public Class ImportExcelBanque

    Private conn As New SqlClient.SqlConnection
    Private OM_BaseCpta As BSCPTAApplication100c
    Private ExcelApp As New Excel.Application

    ' Données modifiées automatiquement à chaque itération de fichier
    Private fileDate As Date ' Date du fichier trouvé à l'interieur de la feuille
    Private reference As String ' Référence du fichier excel
    Private numClient As String ' Code client SAGE eg. CLIENTWEB
    Private sheetName As String ' Nom de la feuille

    ' Connexion à la base
    Private Sub ImportExcelBanque_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If Environment.GetCommandLineArgs.Length <> 4 Then
                Throw New Exception("Nombre d'arguments invalide !")
            Else
                BaseCialAbstract.setDefaultParams(Environment.GetCommandLineArgs.GetValue(1))
                BaseCialAbstract.GetInstance().Open()
                OM_BaseCpta = BaseCialAbstract.GetInstance().CptaApplication
                'OM_BaseCpta.Name = Environment.GetCommandLineArgs.GetValue(1)
                'OM_BaseCpta.Loggable.UserName = Environment.GetCommandLineArgs.GetValue(2)
                'OM_BaseCpta.Loggable.UserPwd = Environment.GetCommandLineArgs.GetValue(3)
                numClient = Environment.GetCommandLineArgs.GetValue(3)
                'OM_BaseCpta.Open()

                conn.ConnectionString = String.Format("server={0};Trusted_Connection=yes;database={1}", OM_BaseCpta.DatabaseInfo.ServerName, OM_BaseCpta.DatabaseInfo.DatabaseName)
                conn.Open()
                Log(String.Format("Base de donnée: {0}, Client: {1}", OM_BaseCpta.Name, numClient) & Environment.NewLine)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
        End Try
    End Sub

    Private Sub TableLayoutPanel1_Paint(sender As Object, e As PaintEventArgs)

    End Sub

    Private Sub TableLayoutPanel1_Paint_1(sender As Object, e As PaintEventArgs)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles fichierExcelButton.Click
        If fichierExcelDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim i As Integer = 0
            For Each fileName As String In fichierExcelDialog.FileNames
                Log("Chargement du fichier " & fichierExcelDialog.SafeFileNames(i) & Environment.NewLine)
                Dim book As Excel.Workbook = ExcelApp.Workbooks.Open(fileName)

                'Vérification du fichier excel
                If verifStructure(book, fileName) = True Then
                    If loadDatas(book.Worksheets(sheetName)) = True Then
                        book.Close(False)
                        archiveFile(fileName)
                    End If
                End If

                i = i + 1
            Next
        End If
    End Sub

    Private Sub archiveFile(ByVal fileName As String)
        ' Deplace le fichier dans Archives
        Dim archivePath As String = Path.GetDirectoryName(fileName) & "\Archives\" & Path.GetFileName(fileName)
        Try
            If File.Exists(archivePath) Then
                ' Le fichier existe déjà, on supprime
                File.Delete(fileName)
            Else
                ' On deplace le cas echeant
                File.Move(fileName, archivePath)
            End If
        Catch ex As Exception
            Log("Deplacement/Suppression du fichier impossible: " & ex.Message, Color.Red)
        End Try
    End Sub

    Private Sub fichierExcelDialog_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles fichierExcelDialog.FileOk

    End Sub

    Private Sub TableLayoutPanel1_Paint_2(sender As Object, e As PaintEventArgs) Handles TableLayoutPanel1.Paint

    End Sub

    ' Vérifie la structure et assigne des variables globales
    Private Function verifStructure(ByVal book As Excel.Workbook, ByVal fileName As String) As Boolean
        Log("Vérification de la stucture" & Environment.NewLine)

        sheetName = Replace(book.Name, ".xls", "")

        ' Nom de fichier incorrect
        If Split(sheetName, "_")(0) <> "rapprochements" Then
            Log("Nom de fichier incorrect" & Environment.NewLine, Color.Red)
        End If

        ' Vérif nom de la feuille
        If Not containing(book.Worksheets, sheetName) Then
            Log("Feuille rapprochements non trouvée" & Environment.NewLine, Color.Red)
            Return False
        End If

        Dim sheet As Excel.Worksheet = book.Worksheets(sheetName)

        ' Vérif Ligne 1
        If sheet.Cells(1, 1).Value <> "TITRE" _
            Or sheet.Cells(1, 2).Value <> "bodinquincaillerie" _
            Or sheet.Cells(1, 3).Value = "" Then
            Log("Structure incorrecte Ligne 1" & Environment.NewLine, Color.Red)
            Return False
        End If

        If sheet.Cells(1, 4).Value <> "V1" Then
            Log("Cellule D1: Version <> V1" & Environment.NewLine, Color.Red)
            Return False
        End If

        'Vérif date fichier
        Dim dateString As String = sheet.Cells(1, 3).Value.ToString
        Dim format As String = "yy/MM/dd_hh:mm:ss"
        Try
            fileDate = DateTime.ParseExact(dateString, format, Nothing)
        Catch e As FormatException
            Log($"Cellule C1: {dateString} a un format incorrect" & Environment.NewLine, Color.Red)
            Return False
        End Try

        '@deprecated utilise le numéro de remise à partir du 07/02/2017
        '@see loadDatas()
        reference = "WEB-" & fileDate.ToString("yy-MM-dd")
        If pieceExiste(conn, reference) = True Then
            Log(String.Format("Le fichier a déjà été intégré ({0})", reference) & Environment.NewLine, Color.Red)
            book.Close(False)
            archiveFile(fileName)
            Return False
        End If

        ' Vérif entetes
        If sheet.Cells(2, 1).Value <> "ENTETE" _
            Or sheet.Cells(2, 6).Value <> "TRANSACTION_ID" _
            Or sheet.Cells(2, 15).Value <> "REMITTANCE_DATE" _
            Or sheet.Cells(2, 17).Value <> "BRUT_AMOUNT" _
            Or sheet.Cells(2, 20).Value <> "NET_AMOUNT" _
            Or sheet.Cells(2, 21).Value <> "COMMISSION_AMOUNT" _
            Or sheet.Cells(2, 13).Value <> "OPERATION_TYPE" Then
            Log("Structure incorrecte Ligne 2" & Environment.NewLine, Color.Red)
            Return False
        End If
        Return True
    End Function

    ' This function can be used with any collection like object (Shapes, Range, Names, Workbooks, etc ...)
    Private Function containing(objCollection As Object, strName As String) As Boolean
        Dim o As Object
        On Error Resume Next
        o = objCollection(strName)
        containing = (Err.Number = 0)
    End Function

    Private Function loadDatas(ByVal sheet As Excel.Worksheet) As Boolean

        Try

            Dim listeEC As New List(Of LigneEC)

            ' Premier traitement : Parse le fichier excel dans une liste
            Dim rc As Integer = sheet.Rows.Count
            For i As Integer = 3 To rc
                If sheet.Cells(i, 1).Value = "FIN" Then
                    Exit For
                End If

                listeEC.Add(
                    New LigneEC() With {
                        .trId = sheet.Cells(i, 6).Value,
                        .opeType = sheet.Cells(i, 13).Value,
                        .brutAmount = sheet.Cells(i, 17).Value,
                        .remDate = DateTime.ParseExact(sheet.Cells(i, 15).Value, "yyyyMMdd", Nothing),
                        .remId = sheet.Cells(i, 19).Value,
                        .netAmount = sheet.Cells(i, 20).Value,
                        .comAmount = sheet.Cells(i, 21).Value
                })
            Next

            ' Deuxième traitement : regroupe par numero remise
            Dim groupLignes = From l In listeEC Group By l.remId Into lignes = Group, Count(), MaxDate = Max(l.remDate)

            For Each lg In groupLignes
                Log(Environment.NewLine)
                Log(String.Format("Remise n° {0}, {1} lignes, date : {2}", lg.remId, lg.Count, lg.MaxDate.ToShortDateString) & Environment.NewLine)

                ' Jusqu'au 18/12/18 ecRef = WEB-remId
                ' Depuis le 18/12/18 ecRef = WEB-fileDate-remId
                Dim ecRef As String = "WEB-" & fileDate.ToString("yyMMdd") & "-" & lg.remId
                ' Si la pièce existe déjà on passe
                If pieceExiste(conn, ecRef) Then
                    Log($"La pièce référence '{ecRef}' a déjà été importée" & Environment.NewLine, Color.Red)
                    Continue For
                End If

                Dim totalCom As Double = 0
                Dim totalCreditNet As Double = 0
                Dim totalDebitNet As Double = 0
                Dim totalAmount As Double = 0
                Dim EcritureG As IBOEcriture3
                Dim trIds As List(Of KeyValuePair(Of String, String)) = New List(Of KeyValuePair(Of String, String))

                Dim OM_PieceComptable As IPMEncoder = CreePieceComptable(OM_BaseCpta, "BNP", lg.MaxDate, String.Empty, ecRef)

                For Each ligne In lg.lignes

                    If ligne.opeType = "DT" Then
                        totalCom += ligne.comAmount
                        totalAmount += ligne.brutAmount
                        totalCreditNet += ligne.netAmount

                        ligne.brutAmount = ligne.brutAmount / 100
                        Log(String.Format("TRID: {0}, Montant: {1}, Type: ACPTE", ligne.trId, ligne.brutAmount) & Environment.NewLine)
                        EcritureG = AjouteLigneEcritureGenerale(OM_PieceComptable, "411000", numClient, "512000", String.Format("{0}CB WEB", ligne.trId), EcritureSensType.EcritureSensTypeCredit, ligne.brutAmount, "C.Bleue", ligne.remDate)
                    ElseIf ligne.opeType = "CT" Then
                        totalDebitNet += ligne.netAmount
                        ligne.netAmount = ligne.netAmount / 10000
                        Log(String.Format("TRID: {0}, Montant: {1}, Type: REMB", ligne.trId, ligne.netAmount) & Environment.NewLine)
                        EcritureG = AjouteLigneEcritureGenerale(OM_PieceComptable, "411000", numClient, "512000", String.Format("{0}REMB CB WEB", ligne.trId), EcritureSensType.EcritureSensTypeDebit, ligne.netAmount, "C.Bleue", ligne.remDate)
                    Else
                        Throw New Exception(String.Format("Operation type '{0}' non pris en charge", ligne.opeType))
                    End If

                    ' Insère dans un tableau les numéros de transaction pour retraitement après validation
                    ' car le process n'enregistre pas le EC_Reference dans chaque ligne
                    trIds.Add(New KeyValuePair(Of String, String)(ligne.trId, EcritureG.EC_Intitule))
                Next

                If totalCom > 0 Then
                    totalCom = totalCom / 100
                    Log(String.Format("TOTAL COM: {0}", totalCom) & Environment.NewLine)
                    AjouteLigneEcritureGenerale(OM_PieceComptable, "627611", String.Empty, "512000", "COM CB WEB", EcritureSensType.EcritureSensTypeDebit, totalCom, String.Empty, lg.MaxDate)
                End If

                If totalCreditNet > 0 Then
                    totalCreditNet = totalCreditNet / 10000
                    Log(String.Format("TOTAL NET: {0}", totalCreditNet) & Environment.NewLine)
                    AjouteLigneEcritureGenerale(OM_PieceComptable, "512000", String.Empty, "411000", "CB NET WEB", EcritureSensType.EcritureSensTypeDebit, totalCreditNet, String.Empty, lg.MaxDate)
                End If

                If totalDebitNet > 0 Then
                    totalDebitNet = totalDebitNet / 10000
                    Log(String.Format("TOTAL REMB: {0}", totalDebitNet) & Environment.NewLine)
                    AjouteLigneEcritureGenerale(OM_PieceComptable, "512000", String.Empty, "411000", "CB REMB WEB", EcritureSensType.EcritureSensTypeCredit, totalDebitNet, String.Empty, lg.MaxDate)
                End If

                totalAmount = totalAmount / 100
                Log(String.Format("Total Montant: {0}, total com: {1}, total net: {2}, total remb: {3}", totalAmount, totalCom, totalCreditNet, totalDebitNet) & Environment.NewLine)
                ValidePieceComptable(OM_PieceComptable)
                ' Retraite les Ecritures
                For Each ec As IBOEcriture3 In OM_PieceComptable.ListEcrituresOut
                    For Each pair As KeyValuePair(Of String, String) In trIds
                        Dim trId As String = pair.Key
                        Dim intitule As String = pair.Value
                        If intitule = ec.EC_Intitule Then
                            ec.EC_Reference = trId
                            ec.EC_Intitule = Replace(ec.EC_Intitule, trId, "")
                            ec.Write()
                        End If
                    Next
                Next
            Next

            Log("Import Fichier terminé." & Environment.NewLine)
            Return True
        Catch ex As Exception
            Log(ex.Message & Environment.NewLine, Color.Red)
            Return False
        End Try
    End Function

    Private Sub Log(message As String, Optional color As Color = Nothing)
        If color = Nothing Then
            color = Color.Black
        End If
        logBox.Select(logBox.TextLength, 0)
        logBox.SelectionColor = color
        logBox.AppendText(message)
    End Sub

    Private Sub CloseBtn_Click(sender As Object, e As EventArgs) Handles CloseBtn.Click
        ExitApp()
    End Sub

    Private Sub ImportExcelBanque_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        ExitApp()
    End Sub

    Private Sub ExitApp()
        ExcelApp.Quit()
        Application.Exit()
        BaseCialAbstract.GetInstance.Close()
    End Sub
End Class
