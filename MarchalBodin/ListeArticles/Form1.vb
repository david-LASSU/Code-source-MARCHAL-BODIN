Imports System.IO
Imports System.Data.SqlClient
Imports System.Text
Imports Microsoft.Win32

Public Class Form1
    Private programme As String
    Private fichierGescom As String
    Private cnxString As String
    Private categorieTable As New DataTable
    Private memoryCache As Cache
    Private dontRunHandler As Boolean = False

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If Environment.GetCommandLineArgs.Count < 2 Then
                Throw New Exception("Nombre d'arguments insuffisant pour démarrer")
            End If
            ' Recup arguments
            fichierGescom = Environment.GetCommandLineArgs.GetValue(1)
            programme = Environment.GetCommandLineArgs.GetValue(2)

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

            ' Crée la chaine de connexion
            cnxString = String.Format("server={0};Trusted_Connection=yes;database={1};MultipleActiveResultSets=True", server, dbName)
            Text = "Liste des articles " + dbName

            ArtStatusComboBox.SelectedIndex = 0
            RefComboBox.SelectedIndex = 0
            FournPrincComboBox.SelectedIndex = 0

            ' Handle Enter key of all TextBox inside Panel1
            For Each ctrl As TextBox In Panel1.Controls.OfType(Of TextBox)
                AddHandler ctrl.KeyDown, AddressOf TextBox_KeyDown
            Next

            InitFournisseur()

            CreateCategorieTree()

            ToolTip1.SetToolTip(DesignTextBox, "Ne pas inclure les mots de moins de 3 lettres")

        Catch ex As Exception
            MsgBox(ex.Message)
            Application.Exit()
        End Try
    End Sub

#Region "Fournisseur"
    Private Sub InitFournisseur()
        ' Prépare le combobox fournisseur
        Dim cnx As SqlConnection = New SqlConnection(cnxString)
        cnx.Open()
        Dim cmd As SqlCommand = cnx.CreateCommand
        cmd.CommandText = "SELECT CT_Num, CT_Intitule FROM F_COMPTET WHERE CT_Type = 1 AND CT_Sommeil = 0 ORDER BY CT_Intitule"
        Dim reader As SqlDataReader = cmd.ExecuteReader
        Dim fourns As New AutoCompleteStringCollection

        While reader.Read
            fourns.Add(String.Format("[{0}] {1}", reader.Item("CT_Num"), reader.Item("CT_Intitule")))
        End While

        With FournTextBox
            .AutoCompleteCustomSource = fourns
            .AutoCompleteMode = AutoCompleteMode.None
            .AutoCompleteSource = AutoCompleteSource.CustomSource
        End With
        reader.Close()
        cmd.Dispose()
    End Sub

    ' Ouvre la liste de propositions
    Private Sub FournTextBox_TextChanged(sender As Object, e As EventArgs) Handles FournTextBox.TextChanged
        FournListBox.Items.Clear()
        If FournTextBox.Text.Length = 0 Then
            HideFournResult()
            Exit Sub
        End If

        For Each s As String In FournTextBox.AutoCompleteCustomSource
            If s.IndexOf(FournTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
                Dim i As Integer = FournListBox.Items.Add(s)
            End If
        Next

        If FournListBox.Items.Count > 0 Then
            Dim p As Point = FournTextBox.FindForm().PointToClient(FournTextBox.Parent.PointToScreen(FournTextBox.Location))
            p.Y += FournTextBox.Height
            With FournListBox
                .SelectedIndex = 0
                .BringToFront()
                .Location = p
                .Visible = True
            End With
        Else
            HideFournResult()
        End If

        FournPrincComboBox.Enabled = FournTextBox.AutoCompleteCustomSource.Contains(FournTextBox.Text)
    End Sub

    Private Sub FournTextBox_Leave(sender As Object, e As EventArgs) Handles FournTextBox.Leave
        If ActiveControl.Name <> FournTextBox.Name And ActiveControl.Name <> FournListBox.Name Then
            If Not FournTextBox.AutoCompleteCustomSource.Contains(FournTextBox.Text) Then
                FournTextBox.Text = String.Empty
            End If
            HideFournResult()
        End If
    End Sub

    Private Sub FournListBox_LostFocus(sender As Object, e As EventArgs) Handles FournListBox.LostFocus
        HideFournResult()
    End Sub

    Private Sub FournTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles FournTextBox.KeyDown
        If FournListBox.Visible = True Then
            If e.KeyCode = Keys.Down Then
                If FournListBox.SelectedIndex < FournListBox.Items.Count - 1 Then
                    FournListBox.SelectedIndex += 1
                End If
            End If

            If e.KeyCode = Keys.Up Then
                If FournListBox.SelectedIndex > 0 Then
                    FournListBox.SelectedIndex -= 1
                End If
            End If

            If e.KeyCode = Keys.Escape Then
                HideFournResult()
            End If

            If e.KeyCode = Keys.Enter Then
                FournTextBox.Text = FournListBox.Items(FournListBox.SelectedIndex).ToString
                HideFournResult()
            End If
        End If
    End Sub

    Private Sub FournListBox_KeyDown(sender As Object, e As KeyEventArgs) Handles FournListBox.KeyDown
        If FournListBox.Visible = True Then
            If e.KeyCode = Keys.Enter Then
                FournTextBox.Text = FournListBox.Items(FournListBox.SelectedIndex).ToString
                HideFournResult()
            End If
            If e.KeyCode = Keys.Escape Then
                HideFournResult()
            End If
        End If
    End Sub

    Private Sub FournTextBox_KeyUp(sender As Object, e As KeyEventArgs) Handles FournTextBox.KeyUp
        If FournListBox.Visible = True Then
            If e.KeyCode = Keys.Up Then
                FournTextBox.SelectionStart = FournTextBox.Text.Length + 1
            End If
        End If
    End Sub

    Private Sub FournTextBox_MouseWheel(sender As Object, e As MouseEventArgs) Handles FournTextBox.MouseWheel
        If e.Delta > 0 Then
            'Scroll up
            If FournListBox.SelectedIndex > 0 Then
                FournListBox.SelectedIndex -= 1
            End If
        Else
            'Scroll down
            If FournListBox.SelectedIndex < FournListBox.Items.Count - 1 Then
                FournListBox.SelectedIndex += 1
            End If
        End If
    End Sub

    ''' <summary>
    ''' Auto highlight item on mouse move
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub FournListBox_MouseMove(sender As Object, e As MouseEventArgs) Handles FournListBox.MouseMove
        Dim index As Integer = FournListBox.IndexFromPoint(e.Location)
        If index > -1 Then
            FournListBox.SelectedIndex = index
        End If
    End Sub

    Private Sub FournListBox_MouseClick(sender As Object, e As MouseEventArgs) Handles FournListBox.MouseClick
        If FournListBox.SelectedIndex > -1 Then
            FournTextBox.Text = FournListBox.Items(FournListBox.SelectedIndex).ToString
        End If
        HideFournResult()
        FournTextBox.Focus()
    End Sub

    Private Sub HideFournResult()
        FournListBox.Visible = False
        FournTextBox.SelectionStart = FournTextBox.Text.Length + 1
        FournLabel.Enabled = FournTextBox.AutoCompleteCustomSource.Contains(FournTextBox.Text)
    End Sub

    Private Sub FournListBox_DrawItem(sender As Object, e As DrawItemEventArgs) Handles FournListBox.DrawItem

        If e.Index < 0 Then
            Exit Sub
        End If

        e.DrawBackground()
        e.DrawFocusRectangle()

        Dim text As String
        Dim x As Single = e.Bounds.X, y As Single = e.Bounds.Y

        text = FournListBox.Items(e.Index).ToString
        Dim i As Integer = FournListBox.Items(e.Index).IndexOf(FournTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase)
        Dim boldPos As New List(Of Integer)
        Do While (i <> -1)
            For a = i To i + FournTextBox.Text.Length - 1
                boldPos.Add(a)
            Next
            i = FournListBox.Items(e.Index).IndexOf(FournTextBox.Text, i + 1, StringComparison.CurrentCultureIgnoreCase)
        Loop

        Dim startPoint As New Point(x, y)
        Dim maxSize As New Size(Integer.MaxValue, Integer.MaxValue)
        For i = 0 To text.Length - 1
            Dim f As Font = e.Font
            If boldPos.Contains(i) Then
                f = New Font(e.Font, FontStyle.Bold)
            End If
            Dim sf As Size = TextRenderer.MeasureText(e.Graphics, text.Substring(i, 1), f, maxSize, TextFormatFlags.NoPadding)
            Dim rect As New Rectangle(startPoint, sf)
            TextRenderer.DrawText(e.Graphics, text.Substring(i, 1), f, startPoint, Color.Black, TextFormatFlags.NoPadding)
            startPoint.X += sf.Width
        Next
    End Sub

    Private Sub FournLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles FournLabel.LinkClicked
        Dim ctNum As String = GetCtNum()
        If GetCtNum() <> String.Empty Then
            Dim sagePath As String = GetSagePath()
            If sagePath <> String.Empty Then
                Process.Start(sagePath, String.Format("""{0}"" -cmd=""Tiers.Show(Tiers='{1}')""", fichierGescom, ctNum))
            End If
        Else
            MsgBox("Vous devez selectionner un fournisseur")
        End If
    End Sub

    Public Function GetCtNum() As String
        Dim r As New RegularExpressions.Regex("^\[(.*)\]")

        If r.IsMatch(FournTextBox.Text) Then
            Dim ctNum As String = r.Match(FournTextBox.Text).Groups(1).Value
            Return ctNum
        End If

        Return String.Empty
    End Function
#End Region

#Region "Référence"

    Private Sub RefComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles RefComboBox.SelectedIndexChanged
        Select Case RefComboBox.SelectedIndex
            Case Is = 0
                RefTextBox1.Enabled = False
                RefTextBox2.Enabled = False
                RefEtLabel.Enabled = False
            Case Is = 2
                RefTextBox1.Enabled = True
                RefTextBox2.Enabled = True
                RefEtLabel.Enabled = True
            Case 1, 3 To 10
                RefTextBox1.Enabled = True
                RefTextBox2.Enabled = False
                RefEtLabel.Enabled = False
        End Select
    End Sub

#End Region

#Region "Résultats"

    Private _sortCol As String = "AR_Ref"
    Private _sortWay As Windows.Forms.SortOrder = Windows.Forms.SortOrder.Ascending

    Public Property SortCol As String
        Get
            Return _sortCol
        End Get
        Set(value As String)
            _sortCol = value
        End Set
    End Property

    Public Property SortWay As Windows.Forms.SortOrder
        Get
            Return _sortWay
        End Get
        Set(value As Windows.Forms.SortOrder)
            _sortWay = value
        End Set
    End Property

    ''' <summary>
    ''' Actions au simple clic de souris sur une entete de colonne
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridView1_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.ColumnHeaderMouseClick
        If e.Button = MouseButtons.Left Then
            ' Modifie l'ordre de tri au clic gauche
            Dim colName As String = DataGridView1.Columns(e.ColumnIndex).Name
            SortWay = If(SortCol = colName AndAlso _sortWay = Windows.Forms.SortOrder.Ascending, Windows.Forms.SortOrder.Descending, Windows.Forms.SortOrder.Ascending)
            SortCol = DataGridView1.Columns(e.ColumnIndex).Name
            getList()
        End If
    End Sub

    ''' <summary>
    ''' Clic souris sur une cellule
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridView1_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseClick

        If e.Button = MouseButtons.Right And e.RowIndex > -1 And e.ColumnIndex > -1 Then
            DataGridView1.ClearSelection()
            DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex).Selected = True
            Dim artRepo = New ArticleRepository(cnxString)
            Dim table As DataTable = artRepo.getCatTarif(DataGridView1.Item("AR_Ref", e.RowIndex).Value.ToString)
            Dim pourcentage As String
            Dim pxCat As Double
            Dim conditionnement As String

            With ContextMenuStrip1
                .Items.Clear()
                For Each row As DataRow In table.Rows
                    With row.Item("AC_Categorie")
                        If .ToString = "7" Or .ToString = "14" Then
                            pourcentage = "------"
                        Else
                            pourcentage = Math.Round(IIf(IsDBNull(row.Item("AC_Remise")), 0, row.Item("AC_Remise")), 2) & "%"
                        End If
                    End With

                    pxCat = If(IsDBNull(row.Item("pxCat")), 0, Math.Round(row.Item("pxCat"), 2))

                    conditionnement = row.Item("Conditionnement") & "::" & row.Item("CT_Intitule")
                    .Items.Add(String.Format("{0} : {1} : {2}€", conditionnement, pourcentage, pxCat))
                Next

                .Show(DataGridView1, DataGridView1.PointToClient(Cursor.Position))
            End With
        ElseIf e.Button = MouseButtons.Right And e.RowIndex = -1 And e.ColumnIndex = -1
            ' Reset column params
            With ContextMenuStrip1
                .Items.Clear()
                .Items.Add("Ordre de colonnes par défaut")
                .Show(DataGridView1, DataGridView1.PointToClient(Cursor.Position))
            End With
        End If
    End Sub

    Private Sub ContextMenuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ContextMenuStrip1.ItemClicked
        Select Case True
            ' Reset le positionnement des colonnes
            Case e.ClickedItem.Text = "Ordre de colonnes par défaut"
                My.Settings.Dgv1OrderCols = Nothing
                My.Settings.Save()
                DataGridView1.Columns.Clear()
                getList()
                Exit Select
        End Select
    End Sub

    ''' <summary>
    ''' Sauvegarde le positionnement des colonnes
    ''' </summary>
    Private Sub SaveOrderCols()
        If dontRunHandler = True Then
            Exit Sub
        End If
        If My.Settings.Dgv1OrderCols Is Nothing Then
            My.Settings.Dgv1OrderCols = New Specialized.StringCollection
        End If
        My.Settings.Dgv1OrderCols.Clear()
        For Each c As DataGridViewColumn In DataGridView1.Columns
            My.Settings.Dgv1OrderCols.Add(c.Name & "#" & c.DisplayIndex & "#" & c.Width)
        Next
        My.Settings.Save()
    End Sub

    Private Sub SearchButton_Click(sender As Object, e As EventArgs) Handles SearchButton.Click
        getList()
    End Sub

    Private Sub TextBox_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            getList()
        End If
    End Sub

    Private Function ContainsTwoChars(ByVal s As string) As Boolean
        Return s.Length < 3
    End Function

    ''' <summary>
    ''' Affiche le résultat de la requete en fonction du formulaire
    ''' </summary>
    Private Sub getList()
        Try
            ' TODO deplacer l'appel de cette methode sur un evenemment
            ' qui detecte le deplacement/resize des colonnes
            'SaveOrderCols()

            Dim text As List(Of String) = DesignTextBox.Text.Split(" ").ToList
            text.RemoveAll(AddressOf ContainsTwoChars)
            DesignTextBox.Text = String.Join(" ", text.ToArray)
            Application.DoEvents

            'DataGridView1.Visible = False
            DataGridView1.DataSource = Nothing
            ' Set le rowCount à 0 pour virtual mode
            DataGridView1.RowCount = 0

            Dim articleRepos As New ArticleRepository(cnxString)

            memoryCache = New Cache(articleRepos, 50)

            If (DataGridView1.ColumnCount = 0) Then

                For Each column As DataColumn In articleRepos.Columns
                    DataGridView1.Columns.Add(column.ColumnName, column.ColumnName)
                Next

                PrepareDataGridColumns()
            End If

            ' Place la flèche de Tri sur la bonne colonne
            For Each c As DataGridViewColumn In DataGridView1.Columns
                c.HeaderCell.SortGlyphDirection = Windows.Forms.SortOrder.None
                If c.Name = SortCol Then
                    c.HeaderCell.SortGlyphDirection = SortWay
                End If
            Next

            ' Repositionne les colonnes dans leur derniere position et largeur connue
            dontRunHandler = True
            If My.Settings.Dgv1OrderCols IsNot Nothing Then
                For Each colNameIndex As String In My.Settings.Dgv1OrderCols
                    Dim tab() As String = colNameIndex.Split("#")
                    If DataGridView1.Columns.Contains(tab(0)) AndAlso tab(1) <= DataGridView1.ColumnCount - 1 Then
                        DataGridView1.Columns(tab(0)).DisplayIndex = tab(1)
                        DataGridView1.Columns(tab(0)).Width = tab(2)
                    End If
                Next
            Else
                ' Resize les colonnes
                DataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnMode.DisplayedCells)
            End If
            dontRunHandler = False

            ' Met à jour le nombre de résultats
            DataGridView1.RowCount = articleRepos.RowCount
            NbResultsLabel.Text = String.Format("{0} résultat{1}", DataGridView1.RowCount, If(DataGridView1.RowCount > 1, "s", ""))
            NbResultsLabel.Visible = True

            DataGridView1.Visible = True
        Catch ex As Exception
            Debug.Print(ex.ToString)
        End Try
    End Sub

    Private Sub dataGridView1_CellValueNeeded(ByVal sender As Object, ByVal e As DataGridViewCellValueEventArgs) Handles DataGridView1.CellValueNeeded
        'Debug.Print("cell value needed: " & e.RowIndex & ":" & e.ColumnIndex)
        e.Value = memoryCache.RetrieveElement(e.RowIndex, e.ColumnIndex)
    End Sub

    Private Sub PrepareDataGridColumns()

        With DataGridView1
            .ReadOnly = True

            With .Columns("AR_Ref")
                .HeaderText = "Référence"
            End With
            With .Columns("SUPPRIME")
                .HeaderText = "Supprimé"
            End With
            With .Columns("SUPPRIME_USINE")
                .HeaderText = "Supprimé usine"
            End With
            With .Columns("AR_Design")
                .HeaderText = "Libellé"
            End With
            With .Columns("AR_CodeBarre")
                .HeaderText = "Code Barre"
            End With
            With .Columns("AF_RefFourniss")
                .HeaderText = "Ref Fourn"
            End With
            With .Columns("FA_CodeFamille")
                .HeaderText = "Famille"
            End With
            With .Columns("CT_Num")
                .HeaderText = "Fournisseur principal"
            End With
            With .Columns("U_Intitule")
                .HeaderText = "Unite Vente"
            End With
            With .Columns("CT_Intitule")
                .HeaderText = "Nom fournisseur principal"
                .Visible = False
            End With
            With .Columns("AR_PrixVen")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Prix vente HT"
            End With
            With .Columns("AC_PrixVen")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Prix vente TTC"
            End With
            With .Columns("DP_Code")
                .HeaderText = "Emplacement principal"
            End With
            With .Columns("SuiviEnStock")
                .HeaderText = "Suivi en stock"
            End With
            With .Columns("QteSto")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Stock réel"
            End With
            'With .Columns("STO_DISPO")
            '    .DefaultCellStyle.Format = "N2"
            '    .HeaderText = "Disponible"
            'End With
            With .Columns("StockATerme")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Stock à terme"
            End With
            With .Columns("QtePrepa")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Préparé"
            End With
            With .Columns("QteCom")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Commandé"
            End With
            With .Columns("QteRes")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Réservé"
            End With
            With .Columns("QteMini")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Stock Min"
            End With
            With .Columns("QteMaxi")
                .DefaultCellStyle.Format = "N2"
                .HeaderText = "Stock Max"
            End With
            With .Columns("refInterne")
                .HeaderText = "Ref Interne"
            End With
            Dim cs As New DGVColumnSelector.DataGridViewColumnSelector(DataGridView1, "Dgv1VisibleCols")
            For Each c As DataGridViewColumn In DataGridView1.Columns
                c.Visible = My.Settings.Dgv1VisibleCols.Contains(c.HeaderText)
            Next
        End With
    End Sub

    ''' <summary>
    ''' Effectue certaines action au double clic d'une cellule
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridView1_CellMouseDoubleClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseDoubleClick
        If e.Button = MouseButtons.Left And e.RowIndex > -1 And e.ColumnIndex > -1 Then
            Dim sagePath As String = GetSagePath()
            If sagePath = String.Empty Then
                Exit Sub
            End If

            Select Case e.ColumnIndex
                Case DataGridView1.Columns("CT_Num").Index,
                     DataGridView1.Columns("CT_Intitule").Index
                    ' Si colonne fournisseur on ouvre la fiche fournisseur
                    Process.Start(sagePath, String.Format("""{0}"" -cmd=""Tiers.Show(Tiers='{1}')""", fichierGescom, DataGridView1.Rows(e.RowIndex).Cells("CT_Num").Value.ToString))
                Case DataGridView1.Columns("QteSto").Index
                    Process.Start(sagePath, String.Format("""{0}"" -cmd=""InterroArticle.Show(Masque=Stocks,Article='{1}')""", fichierGescom, DataGridView1.Rows(e.RowIndex).Cells("AR_Ref").Value.ToString))
                Case DataGridView1.Columns("StockATerme").Index,
                     DataGridView1.Columns("QtePrepa").Index,
                     DataGridView1.Columns("QteCom").Index,
                     DataGridView1.Columns("QteRes").Index
                    Process.Start(sagePath, String.Format("""{0}"" -cmd=""InterroArticle.Show(Masque=StocksPrevisionnels,Article='{1}')""", fichierGescom, DataGridView1.Rows(e.RowIndex).Cells("AR_Ref").Value.ToString))
                Case Else
                    ' Sinon par ouvre article par defaut
                    Process.Start(sagePath, String.Format("""{0}"" -cmd=""Article.Show(Article='{1}')""", fichierGescom, DataGridView1.Rows(e.RowIndex).Cells("AR_Ref").Value.ToString))
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Personnalisation des colonnes en fonction des résultats
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridView1_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles DataGridView1.CellFormatting
        Dim f As Font = DataGridView1.DefaultCellStyle.Font

        With DataGridView1.Rows(e.RowIndex)
            If .Cells("Sommeil").Value.ToString = "Oui" _
                Or .Cells("SUPPRIME").Value.ToString = "Oui" _
                Or .Cells("SUPPRIME_USINE").Value.ToString = "Oui" Then

                '.DefaultCellStyle.ForeColor = Color.Gray
                .DefaultCellStyle.Font = New Font(f, FontStyle.Strikeout)
            Else
                .DefaultCellStyle.Font = f
                '.DefaultCellStyle.ForeColor = Color.Black
            End If
        End With

        If e.ColumnIndex = DataGridView1.Columns("CT_Num").Index Then
            If e.Value.ToString <> String.Empty Then
                With DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)
                    .ToolTipText = DataGridView1.Rows(e.RowIndex).Cells("CT_Intitule").Value.ToString
                End With
            End If
        End If

        If e.ColumnIndex = DataGridView1.Columns("SuiviEnStock").Index Then
            With DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)
                .Style.ForeColor = If(e.Value.ToString = "Oui", Color.Green, Color.Red)
            End With
        End If

        If DataGridView1.Columns(e.ColumnIndex).DefaultCellStyle.Format = "N2" Then
            If e.Value.ToString <> String.Empty Then
                e.Value = Convert.ToDouble(e.Value)
                With DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)
                    If e.Value = 0 Then
                        e.Value = vbNullString
                    ElseIf e.Value < 0 Then
                        .Style.ForeColor = Color.Red
                    ElseIf e.ColumnIndex = DataGridView1.Columns("StockATerme").Index AndAlso e.Value < DataGridView1.Rows(e.RowIndex).Cells("QteMini").Value
                        .Style.ForeColor = Color.DarkOrange
                        .ToolTipText = "Stock inférieur au Stock Min"
                    Else
                        .Style.ForeColor = Color.Black
                        .ToolTipText = Nothing
                    End If
                End With
            End If
        End If

    End Sub

    Private Sub DataGridView1_ColumnDisplayIndexChanged(sender As Object, e As DataGridViewColumnEventArgs) Handles DataGridView1.ColumnDisplayIndexChanged
        SaveOrderCols()
    End Sub

    Private Sub DataGridView1_ColumnWidthChanged(sender As Object, e As DataGridViewColumnEventArgs) Handles DataGridView1.ColumnWidthChanged
        SaveOrderCols()
    End Sub

    Private Sub ResetButton_Click(sender As Object, e As EventArgs) Handles ResetButton.Click
        Application.Restart()
        Me.Refresh()
    End Sub
#End Region

#Region "Categories"
    ''' <summary>
    ''' Creée le menu de catégories
    ''' </summary>
    Private Sub CreateCategorieTree()
        Dim cnx As SqlConnection = New SqlConnection(cnxString)
        cnx.Open()
        Dim cmd As SqlCommand = cnx.CreateCommand
        cmd.CommandText = "SELECT CL_No As ID, CL_Intitule AS NAME, CL_NoParent AS PARENT FROM F_CATALOGUE ORDER BY cbCL_Intitule"
        categorieTable.Load(cmd.ExecuteReader)
        categorieTable.Columns.Add("LEVEL", GetType(Integer))
        TreeView1.Nodes.Add("__ALL__", "Tous")
        Dim i As Integer
        For i = 0 To categorieTable.Rows.Count - 1
            Dim ID1 As String = categorieTable.Rows(i).Item("ID").ToString
            categorieTable.Rows(i).Item("LEVEL") = FindLevel(ID1, 0)
        Next
        Dim MaxLevel1 As Integer = CInt(categorieTable.Compute("MAX(LEVEL)", ""))

        For i = 0 To MaxLevel1
            Dim Rows1() As DataRow = categorieTable.Select("LEVEL = " & i)

            For j As Integer = 0 To Rows1.Count - 1
                Dim ID1 As String = Rows1(j).Item("ID").ToString
                Dim Name1 As String = Rows1(j).Item("NAME").ToString
                Dim Parent1 As String = Rows1(j).Item("PARENT").ToString

                If Parent1 = "0" Then
                    TreeView1.Nodes.Add(ID1, Name1)
                Else
                    Dim TreeNodes1() As TreeNode = TreeView1.Nodes.Find(Parent1, True)

                    If TreeNodes1.Length > 0 Then
                        TreeNodes1(0).Nodes.Add(ID1, Name1)
                    End If
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Find Level from categorieTable
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <param name="Level"></param>
    ''' <returns></returns>
    Private Function FindLevel(ByVal ID As String, ByRef Level As Integer) As Integer
        Dim i As Integer

        For i = 0 To categorieTable.Rows.Count - 1
            Dim ID1 As String = categorieTable.Rows(i).Item("ID").ToString
            Dim Parent1 As String = categorieTable.Rows(i).Item("PARENT").ToString

            If ID = ID1 Then
                If Parent1 = "0" Then
                    Return Level
                Else
                    Level += 1
                    FindLevel(Parent1, Level)
                End If
            End If
        Next

        Return Level
    End Function

    ''' <summary>
    ''' Unselect Old Node
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TreeView1_BeforeSelect(sender As Object, e As TreeViewCancelEventArgs) Handles TreeView1.BeforeSelect
        If TreeView1.SelectedNode IsNot Nothing Then
            TreeView1.SelectedNode.NodeFont = New Font(TreeView1.Font, FontStyle.Regular)
        End If
    End Sub

    ''' <summary>
    ''' Call Getlist on Node selection
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        e.Node.NodeFont = New Font(TreeView1.Font, FontStyle.Bold)
        getList()
    End Sub
#End Region

    Private Function GetSagePath() As String
        Dim exe As String = If(programme = "GESCOM", "gecomaes.exe", "scdmaes.exe")
        Dim sagePath As String = Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exe, "", "NONE")
        If sagePath = "NONE" Then
            MsgBox("Impossible de trouver l'executable Sage - Veuillez contacter l'administrateur")
            Return String.Empty
        Else
            Return sagePath
        End If
    End Function

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub
End Class
