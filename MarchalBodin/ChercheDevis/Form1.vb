Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports Microsoft.Win32

Public Class Form1
    Private cnx As New SqlClient.SqlConnection
    Private fichierGescom As String
    Private fichierCompta As String
    Private user As String

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            fichierGescom = Environment.GetCommandLineArgs.GetValue(1)
            fichierCompta = Environment.GetCommandLineArgs.GetValue(2)
            user = Environment.GetCommandLineArgs.GetValue(3)

            ' Détermine le serveur
            Dim server As String = String.Empty
            Using sr As New StreamReader(fichierGescom)
                Dim line As String
                Do While sr.Peek() >= 0
                    line = sr.ReadLine
                    If line Like "ServeurSQL*" Then
                        server = line.Split("=").Last
                        Exit Do
                    End If
                Loop
            End Using
            Dim dbName As String = fichierGescom.Split("\").Last.Split(".").First

            cnx.ConnectionString = String.Format("server={0};Trusted_Connection=yes;database={1};MultipleActiveResultSets=True", server, dbName)
            cnx.Open()
        Catch ex As Exception
            MsgBox(String.Format("Connection Serveur Impossible : {0}", ex.Message))
            Application.Exit()
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim value As String = TextBox1.Text
        DataGridView1.DataSource = Nothing

        If value = "" Then
            MsgBox("Valeur obligatoire")
        Else
            Dim cmd As SqlCommand = cnx.CreateCommand
            cmd.CommandText = "SELECT DO_Piece, DO_Type, NUM_DEVIS FROM F_DOCENTETE WHERE NUM_DEVIS LIKE '%' + @ref + '%' OR DO_Piece LIKE '%' + @ref + '%'"
            cmd.Parameters.AddWithValue("@ref", value)
            Dim reader As SqlDataReader = cmd.ExecuteReader
            If reader.HasRows = False Then
                MsgBox("Aucun document n'a été trouvé")
            Else
                Dim dt As DataTable = New DataTable
                dt.Load(reader)
                dt.Columns.Add("DO_Piece_str", GetType(String))

                Dim typeDoc(9) As String
                typeDoc(0) = "Devis"
                typeDoc(1) = "Bon de commande"
                typeDoc(2) = "Préparation de livraison"
                typeDoc(3) = "Bon de livraison"
                typeDoc(4) = "Bon de retour"
                typeDoc(5) = "Bon d’avoir"
                typeDoc(6) = "Facture"
                typeDoc(7) = "Facture comptabilisée"
                For Each dr As DataRow In dt.Rows
                    dr(3) = typeDoc(dr(1))
                Next

                DataGridView1.DataSource = dt
                With DataGridView1
                    '.AutoGenerateColumns = True
                    .Columns(0).HeaderCell.Value = "N° de pièce"
                    .Columns(1).HeaderCell.Value = "Type Int"
                    .Columns(1).Visible = False
                    .Columns(2).HeaderCell.Value = "N° de devis"
                    .Columns(3).HeaderCell.Value = "Type"
                    .Refresh()
                End With
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Application.Exit()
    End Sub

    Private Sub DataGridView1_CellMouseDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellDoubleClick
        Dim p As New ProcessStartInfo

        Dim refPiece As String = DataGridView1.Rows(e.RowIndex).Cells(0).Value
        Dim typePiece As String = DataGridView1.Rows(e.RowIndex).Cells(1).Value
        Dim typeDoc(8) As String
        typeDoc(0) = "Devis"
        typeDoc(1) = "BonCommandeClient"
        typeDoc(2) = "PreparationLivraison"
        typeDoc(3) = "BonLivraisonClient"
        typeDoc(4) = "BonRetourClient"
        typeDoc(5) = "BonAvoirClient"
        typeDoc(6) = "FactureClient"
        typeDoc(7) = "FactureComptaClient"

        'MsgBox(refPiece & " " & typeDoc(typePiece))
        Dim sagePath As String = Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\gecomaes.exe", "", "NONE")
        If sagePath = "NONE" Then
            MsgBox("Impossible de trouver l'executable Sage - Veuillez contacter l'administrateur")
        Else
            Process.Start(sagePath, String.Format("""{0}"" ""{1}"" -u=""{2}"" -cmd=""Document.Show(Type={4},Piece='{5}')""",
                                                    fichierGescom, fichierCompta, user, typeDoc(typePiece), refPiece))
        End If
        Application.Exit()
    End Sub

    Private Sub Form1_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            Button1_Click(sender, e)
            e.Handled = True
        End If
    End Sub

    Private Sub SplitContainer1_Panel1_Paint(sender As Object, e As PaintEventArgs) Handles SplitContainer1.Panel1.Paint

    End Sub

    Private Sub SplitContainer1_Panel2_Paint(sender As Object, e As PaintEventArgs) Handles SplitContainer1.Panel2.Paint

    End Sub
End Class
