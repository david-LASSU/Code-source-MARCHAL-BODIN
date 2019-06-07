Imports System.IO
Imports System.Text
Imports System.Data.SqlClient
Imports System.Threading
Imports ZSDK_API.Discovery
Imports ZSDK_API.ApiException
Imports ZSDK_API.Comm
Imports ZSDK_API.Printer
Imports System.Drawing.Printing
Imports Zen.Barcode
Imports System.Text.RegularExpressions
Imports MBCore.Model

Public Class Form1
    Private cnx As SqlClient.SqlConnection
    Private etiquetteRepos As EtiquetteRepository
    Private ipAddress As String
    Private port As Integer = 9100
    Private myPrinter As ZebraPrinter
    Private myPrinterConnexion As ZebraPrinterConnection
    Private handledEvents As Boolean = False

    Private emplRegex As New Regex("^(B\d+)?(E\d+)(R\d+)(N\d+)$")
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'MessageBox.Show("Nouveau!! Après chaque impression, une étiquette de fin sera imprimée pour vérifier que toutes les étiquettes sont sorties." +
        '                "Le programme vous demandera de confirmer que cette etiquette est bien sortie. Le cas échéant, le statut des étiquettes restera inchangé.")
        Try
            Dim fichierGescom As String = Environment.GetCommandLineArgs.GetValue(1)

            BaseCialAbstract.setDefaultParams(fichierGescom)
            etiquetteRepos = New EtiquetteRepository

            If (etiquetteRepos.dbName Like "TARIF*") Then
                Throw New Exception("Base TARIF interdite")
            End If

            Text = "Etiquettes " + etiquetteRepos.dbName

            Dim t As New Thread(AddressOf doDiscovery)
            t.Start()

            ActiveControl = valueText

            searchBox.Items.Clear()
            searchBox.Items.Add("Articles modifiées")
            searchBox.Items.Add("Emplacement commence par")
            searchBox.Items.Add("Référence Article commence par")
            searchBox.Items.Add("Numéro de Fournisseur égal")
            searchBox.Items.Add("Code barre Fournisseur commence par")
            searchBox.Items.Add("Référence Fournisseur commence par")
            searchBox.Items.Add("N° de pièce égal")
            searchBox.SelectedIndex = 0

            SommeilCheckBox.Checked = True
            NoEmpVideCheckBox.Checked = False
            SuiviStockCheckBox.Checked = True

            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            DataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect

            handleEvents(Environment.GetCommandLineArgs)

        Catch ex As Exception
            MsgBox(String.Format("Connection Serveur Impossible : {0}", ex.Message))
            Application.Exit()
        End Try
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        'Debug.Print(TabControl1.SelectedIndex)
        If TabControl1.SelectedIndex = 0 Then
            ' TODO init Articles

        Else
            ' Emplacements
            InitEmplacementsTab()
        End If
    End Sub

    Public Sub handleEvents(ByVal args As String())
        For Each arg As String In args
            Dim a As String() = arg.Split(New Char() {"="c})
            Select Case a(0)
                Case "Piece"
                    autoSelectPiece(a(1))
            End Select
        Next
    End Sub

#Region "Localisation Imprimante Zebra"
    Private Sub doDiscovery()

        updateGuiFromWorkerThread("Recherche l'imprimante ...", Color.Orange)
        Try
            Dim printers As DiscoveredPrinter() = NetworkDiscoverer.Multicast(5)
            displayPrintersAndAutoConnect(printers)
        Catch ex As DiscoveryException
            handleDicoverException(ex.Message)
        End Try
    End Sub

    Private Sub displayPrintersAndAutoConnect(ByVal printers As DiscoveredPrinter())
        If printers.Count > 0 Then
            ipAddress = printers(0).Address

            Dim t As New Thread(AddressOf doConnectTcp)
            t.Start()
        Else
            handleDicoverException("Imprimante non trouvée")
        End If
    End Sub

    Private Sub doConnectTcp()
        updateGuiFromWorkerThread("Connection... Veuillez patienter...", Color.Goldenrod)
        Try
            myPrinterConnexion = New TcpPrinterConnection(Me.ipAddress, Me.port)
            threadedConnect(Me.ipAddress)
        Catch generatedExceptionName As ZebraException
            updateGuiFromWorkerThread("COMM Error! Disconnected", Color.Red)
            doDisconnect()
        End Try
    End Sub

    Private Sub threadedConnect(ByVal addressName As String)
        Try
            myPrinterConnexion.Open()
            'Thread.Sleep(1000)
        Catch generatedExceptionName As ZebraPrinterConnectionException
            updateGuiFromWorkerThread("Unable to connect with printer", Color.Red)
            disconnectPrinter()
        Catch generatedExceptionName As Exception
            updateGuiFromWorkerThread("Error communicating with printer", Color.Red)
            disconnectPrinter()
        End Try
        myPrinter = Nothing
        If myPrinterConnexion IsNot Nothing AndAlso myPrinterConnexion.IsConnected() Then
            Try
                myPrinter = ZebraPrinterFactory.GetInstance(myPrinterConnexion)
                Dim pl As PrinterLanguage = myPrinter.GetPrinterControlLanguage
                updateGuiFromWorkerThread(("Printer Language " + pl.ToString), Color.LemonChiffon)
                'Thread.Sleep(1000)
                updateGuiFromWorkerThread(("Imprimante connectée"), Color.YellowGreen)
                'Thread.Sleep(1000)
            Catch generatedExceptionName As ZebraPrinterConnectionException
                updateGuiFromWorkerThread("Unknown Printer Language", Color.Red)
                myPrinter = Nothing
                'Thread.Sleep(1000)
                disconnectPrinter()
            Catch generatedExceptionName As ZebraPrinterLanguageUnknownException
                updateGuiFromWorkerThread("Unknown Printer Language", Color.Red)
                myPrinter = Nothing
                'Thread.Sleep(1000)
                disconnectPrinter()
            End Try
        End If
    End Sub

    Public Sub disconnectPrinter()
        Dim t As New Thread(AddressOf doDisconnect)
        t.Start()
    End Sub

    Private Sub doDisconnect()
        Try
            If myPrinterConnexion IsNot Nothing AndAlso myPrinterConnexion.IsConnected() Then
                updateGuiFromWorkerThread("Disconnecting...", Color.Honeydew)
                myPrinterConnexion.Close()
            End If
        Catch generatedExceptionName As ZebraException
            updateGuiFromWorkerThread("COMM Error! Disconnected", Color.Red)
        End Try
        'Thread.Sleep(1000)
        updateGuiFromWorkerThread("Not Connected", Color.Red)
        myPrinterConnexion = Nothing
        'updateButtonFromWorkerThread(True)
    End Sub

    Public Function getPrinter() As ZebraPrinter
        Return myPrinter
    End Function

    Public Function getPrinterConnection() As ZebraPrinterConnection
        Return myPrinterConnexion
    End Function
    Private Sub handleDicoverException(ByVal message As [String])
        updateGuiFromWorkerThread(message, Color.Red, True)
    End Sub

    Private Delegate Sub MyProgressEventsHandler(ByVal sender As Object, ByVal printers As DiscoveredPrinter())

    Public Sub updateGuiFromWorkerThread(ByVal message As [String], ByVal color As Color, Optional ByVal showRetryDiscover As Boolean = False)
        If Me.IsDisposed = False Then
            Invoke(New StatusEventHandler(AddressOf UpdateUI), New StatusArguments(message, color, showRetryDiscover))
        End If
    End Sub

    'Delegate for the status event handler
    Private Delegate Sub StatusEventHandler(ByVal e As StatusArguments)
    ' The following method updates the status bar
    Private Sub UpdateUI(ByVal e As StatusArguments)
        statusBar.Text = e.message
        statusBar.BackColor = e.color
        If e.showRetryDiscover = True Then
            PrintButton.Enabled = False
            retryDiscoverLabel.Visible = True
        Else
            retryDiscoverLabel.Visible = False
        End If
    End Sub

    ' Status Bar data class - holds the text to be displayed and the background color of the label
    Private Class StatusArguments
        Inherits System.EventArgs
        Public message As [String]
        Public color As Color
        Public showRetryDiscover As Boolean
        Public Sub New(ByVal message As [String], ByVal color As Color, ByVal showRetryDiscover As Boolean)
            Me.message = message
            Me.color = color
            Me.showRetryDiscover = showRetryDiscover
        End Sub
    End Class

    Private Sub retryDiscoverLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles retryDiscoverLabel.LinkClicked
        Dim t As New Thread(AddressOf doDiscovery)
        t.Start()
    End Sub
#End Region

#Region "Etiquettes article"

    ''' <summary>
    ''' Auto charge les articles depuis un document
    ''' </summary>
    ''' <param name="piece"></param>
    Public Sub autoSelectPiece(ByVal piece As String)
        searchBox.SelectedIndex = 6
        valueText.Text = piece
        DataGridView2.Rows.Clear()
        DataGridView2.Refresh()

        If GetList() = True Then
            DataGridView1.SelectAll()
            sendToPrint.PerformClick()
        End If
    End Sub

    Private Sub PrintButton_Click(sender As Object, e As EventArgs) Handles PrintButton.Click
        Dim formats As New FormatCollection
        Dim format As Format

        ' Groupe par type d'etiquette (zebra ou normal)
        Dim typeDico As New Dictionary(Of String, List(Of PrintLabel))

        For Each row As DataGridViewRow In DataGridView2.Rows
            If Not row.IsNewRow Then
                Dim frmKey As String = row.Cells("FORMAT").Value.ToString.Split("-")(0).Trim
                If Not formats.hasFormat(frmKey) Then
                    Continue For
                End If

                format = formats.getFormat(frmKey)
                If Not typeDico.ContainsKey(format.type) Then
                    typeDico.Add(format.type, New List(Of PrintLabel))
                End If

                Dim emplacement As String = row.Cells("DP_Code").Value.ToString
                If emplRegex.IsMatch(emplacement) Then
                    emplacement = String.Join(" ", emplRegex.Split(emplacement)).Trim
                End If

                ' .prix = FormatNumber(row.Cells("PrixTTC").Value.ToString, 2), ' imprime un mauvais caractère à la place de l'espace
                typeDico.Item(format.type).Add(
                    New PrintLabel With {
                        .format = format,
                        .refMag = row.Cells("REF_MAG").Value.ToString,
                        .designation = Strings.Left(row.Cells("AR_Design").Value.ToString, 50),
                        .prix = row.Cells("PrixTTC").Value.ToString,
                        .colisage = row.Cells("AF_Colisage").Value.ToString,
                        .refFourn = row.Cells("RefFourn").Value.ToString,
                        .emplacement = emplacement,
                        .gencodeFourn = row.Cells("GencodFourn").Value.ToString,
                        .gencodeMag = row.Cells("GENCOD").Value.ToString,
                        .count = row.Cells("QT").Value,
                        .conditionnement = row.Cells("Conditionnement").Value.ToString.Replace("*", "").Trim
                    }
                )
            End If
        Next

        printLabels(typeDico)

        ' TODO Le thread empêche l'affichage de la liste des imprimantes
        'Dim t As New Thread(AddressOf printLabels)
        't.Start()
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        If cnx IsNot Nothing Then
            cnx.Close()
        End If
        If myPrinterConnexion IsNot Nothing Then
            myPrinterConnexion.Close()
        End If

        Application.Exit()
    End Sub

    Private Sub ListButton_Click(sender As Object, e As EventArgs) Handles SearchButton.Click
        GetList()
    End Sub

    ' Vérifie le formulaire
    Private Function CheckForm() As Boolean
        If valueText.Text = "" And searchBox.SelectedIndex <> 0 Then
            MsgBox("Le champs texte est obligatoire")
            Return False
        End If
        Return True
    End Function

    ' Obtient la liste selon les critères de recherche normaux
    Private Function GetList() As Boolean
        If CheckForm() = False Then
            Return False
        End If

        Try
            Dim filter As New ArticleFilter With {
                .SearchType = searchBox.SelectedIndex,
                .SearchValue = valueText.Text,
                .Sommeil = SommeilCheckBox.Checked,
                .NoEmpVide = NoEmpVideCheckBox.Checked,
                .SuiviStock = SuiviStockCheckBox.Checked,
                .StockPositif = StockPositifCheckBox.Checked
            }

            Dim dt As DataTable = etiquetteRepos.GetArticles(filter)
            If dt.Rows.Count = 0 Then
                DataGridView1.DataSource = Nothing
                countTotalRows()
                MsgBox("Aucun article n'a été trouvé")
                Return False
            End If

            dt.Columns.Add("SUIVISTOCK")
            dt.Columns.Add("SOMMEILTXT")
            dt.Columns.Add("QT")

            dt.Columns("REF_MAG").ReadOnly = True
            dt.Columns("AR_Design").ReadOnly = False
            dt.Columns("PrixTTC").ReadOnly = False
            dt.Columns("AF_Colisage").ReadOnly = False
            dt.Columns("RefFourn").ReadOnly = False
            dt.Columns("DP_Code").ReadOnly = False
            dt.Columns("2nd_EMPLACEMENT").ReadOnly = False
            dt.Columns("AS_QteSto").ReadOnly = False
            dt.Columns("QT").ReadOnly = False
            dt.Columns("FORMAT").ReadOnly = False
            dt.Columns("FORMAT").MaxLength = 255

            Dim formats As New FormatCollection
            For Each dr As DataRow In dt.Rows
                dr("AR_Design") = Strings.Left(dr("AR_Design"), 50)
                dr("PrixTTC") = Math.Round(dr("PrixTTC"), 2)
                dr("AF_Colisage") = Math.Round(dr("AF_Colisage"), 0)
                ' Vire la référence générique
                If Not IsDBNull(dr("RefFourn")) AndAlso dr("RefFourn") Like "FOURN*" Then
                    dr("RefFourn") = String.Empty
                End If
                With formats.getFormat(dr("FORMAT"))
                    dr("FORMAT") = .key & " - " & .description
                End With
                dr("AS_QteSto") = Math.Round(dr("AS_QteSto"), 0)

                dr("SUIVISTOCK") = If(dr("AS_QteMini") > 0, "Oui", "Non")
                dr("SOMMEILTXT") = If(dr("AR_Sommeil") = 0, "Non", "Oui")
                dr("QT") = 1
            Next

            With DataGridView1

                .DataSource = dt
                .Columns("AR_Ref").Visible = False
                .Columns("REF_MAG").HeaderCell.Value = "Référence"
                .Columns("REF_MAG").ReadOnly = True
                .Columns("AR_Design").HeaderCell.Value = "Désignation"
                .Columns("PrixTTC").HeaderCell.Value = "Prix TTC"
                .Columns("AF_Colisage").HeaderCell.Value = "Colisage"
                .Columns("RefFourn").HeaderCell.Value = "Ref Fournisseur"
                .Columns("DP_Code").HeaderCell.Value = "Emplacement"
                .Columns("DP_Code").ReadOnly = True
                .Columns("2nd_EMPLACEMENT").HeaderCell.Value = "2nd Empl"
                .Columns("2nd_EMPLACEMENT").ReadOnly = True
                .Columns("AR_Sommeil").Visible = False
                .Columns("FORMAT").HeaderCell.Value = "Format"
                .Columns("FORMAT").ReadOnly = True
                .Columns("HASH1").Visible = False
                .Columns("HASH2").Visible = False
                .Columns("AS_QteSto").HeaderCell.Value = "Stock"
                .Columns("AS_QteSto").ReadOnly = True
                .Columns("AS_QteMini").Visible = False
                .Columns("ETQSTATUS").Visible = False
                .Columns("CT_Num").Visible = False
                .Columns("GENCOD").Visible = False
                .Columns("GencodFourn").HeaderCell.Value = "Code Barre Fournisseur"
                .Columns("SUIVISTOCK").HeaderCell.Value = "Suivi en stock"
                .Columns("SUIVISTOCK").ReadOnly = True
                .Columns("SOMMEILTXT").HeaderCell.Value = "Sommeil"
                .Columns("SOMMEILTXT").ReadOnly = True

                .Columns("QT").HeaderCell.Value = "Qt à imprimer"
            End With

            countTotalRows()

            ' Auto ajoute à la selection si un seul article trouvé
            If (searchBox.SelectedIndex = 2 Or searchBox.SelectedIndex = 4) And DataGridView1.RowCount = 1 Then
                sendToPrint.PerformClick()
                valueText.Text = ""
            End If

            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
    End Function

    Private Sub DataGridView1_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles DataGridView1.CellFormatting
        If DataGridView1.Rows(e.RowIndex).Cells("ETQSTATUS").Value = 0 Then
            DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.YellowGreen
        Else
            DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.Orange
        End If
    End Sub

    ''' <summary>
    ''' Imprime les étiquettes selon le type
    ''' </summary>
    Sub printLabels(ByVal typeDico As Dictionary(Of String, List(Of PrintLabel)))
        For Each pair As KeyValuePair(Of String, List(Of PrintLabel)) In typeDico
            If pair.Key = "zebra" Then
                PrintZebraLabels(pair.Value)
            ElseIf pair.Key = "normal" Then
                PrintNormalLabels(pair.Value)
            End If
        Next
    End Sub

    Private pLabels As List(Of PrintLabel)
    Private LabelsToPrint As ArrayList
    Private WithEvents LabelsDocToPrint As New Printing.PrintDocument
    Private Sub MakeLabelsToPrint()
        LabelsToPrint = New ArrayList
        For Each label As PrintLabel In pLabels
            For i As Integer = 1 To label.count
                LabelsToPrint.Add(label)
            Next
        Next
    End Sub
    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="printLabels"></param>
    Sub PrintNormalLabels(printLabels As List(Of PrintLabel))
        pLabels = printLabels
        MakeLabelsToPrint()
        Dim dialogPrint As New PrintPreviewDialogSelectPrinter
        AddHandler dialogPrint.MyPrintItemClick, AddressOf MyLabelsPrintItemClicked
        dialogPrint.Document = LabelsDocToPrint
        dialogPrint.ShowDialog()
    End Sub

    Private Sub MyLabelsPrintItemClicked()
        MakeLabelsToPrint()
    End Sub

    Private Sub LabelsDocToPrint_PrintPage(sender As Object, e As PrintPageEventArgs) Handles LabelsDocToPrint.PrintPage
        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim breakPage As Boolean
        Dim pLabel As PrintLabel
        Dim pFormat As FormatNormal
        Dim maxWidth As Integer = LabelsDocToPrint.DefaultPageSettings.PaperSize.Width
        If (maxWidth Mod 2) <> 0 Then
            maxWidth -= 1
        End If
        Dim maxHeight As Integer = LabelsDocToPrint.DefaultPageSettings.PaperSize.Height

        LabelsToPrint.Reverse()

        For i As Integer = LabelsToPrint.Count - 1 To 0 Step -1
            pLabel = LabelsToPrint(i)
            pFormat = pLabel.format

            breakPage = pFormat.DrawLabel(pLabel, e.Graphics, maxWidth, maxHeight, x, y)
            LabelsToPrint.RemoveAt(i)

            If breakPage Then
                Exit For
            End If
        Next
        e.HasMorePages = LabelsToPrint.Count > 0
    End Sub

    Sub PrintZebraLabels(pLabels As List(Of PrintLabel))
        doConnectTcp()
        Try
            For Each pLabel As PrintLabel In pLabels
                ' Send the label if printer is connected
                If (Not (myPrinter) Is Nothing) Then
                    For i As Integer = 1 To pLabel.count
                        getPrinterConnection.Write(Encoding.Default.GetBytes(CType(pLabel.format, FormatZebra).GetZplStringFromLabel(pLabel)))
                        resetEtiquette(pLabel)
                        'Debug.Print(refMag)
                    Next
                End If
            Next

            ' Imprime l'étiquette de fin
            If (Not (myPrinter) Is Nothing) Then
                getPrinterConnection.Write(Encoding.Default.GetBytes("^XA^FO30,70^FR^A0N,60,40^FDFIN^FS^XZ"))
            End If
        Catch ex As Exception
            updateGuiFromWorkerThread(ex.Message, Color.Red)
        End Try
    End Sub

    Private Sub resetEtiquette(ByVal pLabel As PrintLabel)
        'update article hash
        etiquetteRepos.resetEtiquette(pLabel.refMag)
    End Sub

    Private Delegate Sub ClearCallback()
    Private Sub clearSelection()
        Me.DataGridView2.Rows.Clear()
    End Sub

    Private Sub searchBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles searchBox.SelectedIndexChanged
        Select Case searchBox.SelectedIndex
            Case Is = 0
                valueText.Enabled = False
                valueText.Text = String.Empty
                SuiviStockCheckBox.Enabled = False
                SuiviStockCheckBox.Checked = True
                NoEmpVideCheckBox.Enabled = True
            Case Is = 1
                NoEmpVideCheckBox.Enabled = False
                NoEmpVideCheckBox.Checked = False
                valueText.Enabled = True
                ActiveControl = valueText
                SuiviStockCheckBox.Enabled = True
            Case Else
                SuiviStockCheckBox.Enabled = True
                NoEmpVideCheckBox.Enabled = True
                valueText.Enabled = True
                ActiveControl = valueText
        End Select
    End Sub

    Private Sub valueText_KeyDown(sender As Object, e As KeyEventArgs) Handles valueText.KeyDown
        If e.KeyCode = Keys.Enter Then
            GetList()
        End If
    End Sub

    Private Sub countTotalRows()
        ResultatLabel.Text = "Résultat de la recherche : " & DataGridView1.RowCount.ToString & " résultat(s)"

        SelectionLabel.Text = "Fenêtre de sélection : " & DataGridView2.RowCount.ToString & " étiquette(s) à imprimer"
        PrintButton.Enabled = DataGridView2.RowCount > 0
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("S:\\DOCUMENTATION\Procédures\Etiquettes.pdf")
    End Sub

    Private Sub DataGridView2_RowsRemoved(sender As Object, e As DataGridViewRowsRemovedEventArgs) Handles DataGridView2.RowsRemoved
        countTotalRows()
    End Sub

    Private Sub DataGridView2_MouseDown(sender As Object, e As MouseEventArgs) Handles DataGridView2.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Right Then
            Dim ht As DataGridView.HitTestInfo
            ht = Me.DataGridView2.HitTest(e.X, e.Y)
            If ht.RowIndex = -1 Then
                Exit Sub
            End If
            Dim row As DataGridViewRow = DataGridView2.Rows(ht.RowIndex)
            If ht.Type = DataGridViewHitTestType.Cell And row.Cells(ht.ColumnIndex).Selected = True Then
                ContextMenuStrip1.Items.Clear()
                ContextMenuStrip1.Items.Add("Supprimer ligne")
                ContextMenuStrip1.Items.Add("Supprimer ligne et Reset Status")
                Dim ResourceSet As Resources.ResourceSet = My.Resources.ResourceManager.GetResourceSet(Globalization.CultureInfo.CurrentCulture, True, True)
                For Each frm As Format In New FormatCollection
                    ContextMenuStrip1.Items.Add(frm.key + " - " + frm.description)
                Next
                ContextMenuStrip1.Show(DataGridView2, e.Location)
            End If
        End If
    End Sub

    Private Sub ContextMenuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ContextMenuStrip1.ItemClicked
        Select Case True
            Case e.ClickedItem.Text = "Supprimer ligne"
                For Each row As DataGridViewRow In DataGridView2.SelectedRows
                    DataGridView2.Rows.Remove(row)
                Next
                countTotalRows()
                Exit Select
            Case e.ClickedItem.Text = "Supprimer ligne et Reset Status"
                ContextMenuStrip1.Hide()
                Dim result As Integer = MessageBox.Show("Le reset du status aura pour effet de ne plus afficher ces etiquette jusqu'à une prochaine modification de leur contenu. Veuillez confirmer.", "Confirmation", MessageBoxButtons.YesNo)
                If result = DialogResult.Yes Then
                    For Each row As DataGridViewRow In DataGridView2.SelectedRows
                        etiquetteRepos.resetEtiquette(row.Cells("REF_MAG").Value.ToString)
                    Next
                    countTotalRows()
                End If
            Case e.ClickedItem.Text.Contains("FORMAT_")
                For Each row As DataGridViewRow In DataGridView2.SelectedRows
                    row.Cells("FORMAT").Value = e.ClickedItem.Text
                Next
                Exit Select
            Case Else
                Exit Select
        End Select
    End Sub

    Private Sub FormatsLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles FormatsLabel.LinkClicked
        Formats.Show()
    End Sub

    ' Envoie la selection de la grid1 vers la grid 2
    Private Sub sendToPrint_Click(sender As Object, e As EventArgs) Handles sendToPrint.Click
        If DataGridView2.ColumnCount = 0 Then
            For Each c As DataGridViewColumn In DataGridView1.Columns
                DataGridView2.Columns.Add(c.Clone())
            Next
        End If
        Dim row As DataGridViewRow
        For Each dtr As DataGridViewRow In DataGridView1.SelectedRows
            Dim found As Boolean = False
            ' Si valeur trouvée dans la grid 2 on ne réinsère pas
            For Each r As DataGridViewRow In DataGridView2.Rows
                If r.Cells("REF_MAG").Value.Equals(dtr.Cells("REF_MAG").Value) Then
                    found = True
                    Exit For
                End If
            Next
            If found = True Then
                'DataGridView1.Rows.Remove(dtr)
                Continue For
            End If
            row = dtr.Clone
            For Each c As DataGridViewCell In dtr.Cells
                row.Cells(c.ColumnIndex).Value = c.Value
            Next
            DataGridView2.Rows.Add(row)
            'DataGridView1.Rows.Remove(dtr)
        Next
        countTotalRows()
        DataGridView1.ClearSelection()
        DataGridView2.ClearSelection()
    End Sub

    Private Sub DataGridView_CellValidating(sender As DataGridView, e As DataGridViewCellValidatingEventArgs) Handles DataGridView1.CellValidating, DataGridView2.CellValidating
        If sender.Columns(e.ColumnIndex).Name = "PrixTTC" And sender.EditingControl IsNot Nothing Then
            sender.EditingControl.Text = sender.EditingControl.Text.Replace(".", ",")
        End If
    End Sub

    ''' <summary>
    ''' Demande d'auto calibration de l'imprimante
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CalibrateButton_Click(sender As Object, e As EventArgs) Handles CalibrateButton.Click
        doConnectTcp()
        If (Not (myPrinter) Is Nothing) Then
            getPrinterConnection.Write(Encoding.Default.GetBytes("^XA~JC^XZ"))
            MsgBox("Une fois l'opération terminée, si le voyant clignote, appuyez sur le bouton vert")
        End If
    End Sub
#End Region

#Region "Emplacements"
    Private itemsToPrint As ArrayList = Nothing

    Private Sub InitEmplacementsTab()
        Dim emplacements As DataTable = etiquetteRepos.GetEmplacements(EmpFilterText.Text)
        EmpList.DataSource = emplacements
        EmpList.DisplayMember = "DP_Code"
    End Sub

    Private Sub EmpSelectBtn_Click(sender As Object, e As EventArgs) Handles EmpSelectBtn.Click
        For Each item As DataRowView In EmpList.SelectedItems
            If EmpSelectList.Items.Contains(item(0).ToString) Then
                Continue For
            End If

            EmpSelectList.Items.Add(item.Item(0).ToString)
        Next
    End Sub

    Private Sub EmpUnselectBtn_Click(sender As Object, e As EventArgs) Handles EmpUnselectBtn.Click
        Do While EmpSelectList.SelectedItems.Count > 0
            EmpSelectList.Items.Remove(EmpSelectList.SelectedItem)
        Loop
    End Sub

    Private WithEvents EmpDocToPrint As New Printing.PrintDocument

    Private Sub EmpPrintBtn_Click(sender As Object, e As EventArgs) Handles EmpPrintBtn.Click
        itemsToPrint = New ArrayList(EmpSelectList.Items)
        Dim dialogPrint As New PrintPreviewDialogSelectPrinter
        AddHandler dialogPrint.MyPrintItemClick, AddressOf MyEmpPrintItemClicked
        dialogPrint.Document = EmpDocToPrint
        dialogPrint.ShowDialog()
    End Sub

    Private Sub MyEmpPrintItemClicked()
        itemsToPrint = New ArrayList(EmpSelectList.Items)
    End Sub

    Private Sub EmpDocToPrint_PrintPage(sender As Object, e As PrintPageEventArgs) Handles EmpDocToPrint.PrintPage
        Dim f As New Font("Arial", 20, System.Drawing.FontStyle.Regular)
        Dim sf As New StringFormat()
        sf.Alignment = StringAlignment.Center

        Dim marge = 40
        Dim x As Integer = marge
        Dim y As Integer = marge

        ' Permet de s'assurer de prendre l'espace nécessaire pour la longueur max d'une adresse (B20E20R20N20)
        Dim imgMaxWidth As Integer = 265

        Dim imgWithText As Image
        Dim barCode39 As Code39BarcodeDraw = BarcodeDrawFactory.Code39WithoutChecksum
        Dim maxWidth As Integer = EmpDocToPrint.DefaultPageSettings.PaperSize.Width
        Dim maxHeight As Integer = EmpDocToPrint.DefaultPageSettings.PaperSize.Height

        For i As Integer = itemsToPrint.Count - 1 To 0 Step -1
            Dim item As String = itemsToPrint(i)

            ' Generate barcode
            Dim img As Image = barCode39.Draw(item, 45, 1)

            ' Create a new image with extra height for text
            imgWithText = New Bitmap(imgMaxWidth, img.Height + f.Height + 10)

            ' Create a graphics based on image & text
            With Graphics.FromImage(imgWithText)
                .DrawImageUnscaled(img, 0, 0)
                .DrawString(item, f, Brushes.Black, New Rectangle(0, img.Height + 5, img.Width, 30), sf)
            End With

            ' Break line
            If (x + imgWithText.Width) > maxWidth Then
                x = marge
                y += marge + imgWithText.Height
            End If

            ' Break page
            If (y + imgWithText.Height) > maxHeight Then
                Exit For
            End If

            ' Draw the real barcode image + text on page
            e.Graphics.DrawImageUnscaled(imgWithText, x, y)

            x += imgWithText.Width + marge

            itemsToPrint.RemoveAt(i)
        Next
        e.HasMorePages = itemsToPrint.Count > 0
    End Sub

    Private Sub EmpFilterBtn_Click(sender As Object, e As EventArgs) Handles EmpFilterBtn.Click
        InitEmplacementsTab()
    End Sub

    Private Sub EmpFilterText_KeyDown(sender As Object, e As KeyEventArgs) Handles EmpFilterText.KeyDown
        If e.KeyCode = Keys.Enter Then
            InitEmplacementsTab()
        End If
    End Sub
#End Region
End Class
