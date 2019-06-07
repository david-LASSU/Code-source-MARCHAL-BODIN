<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.PrintButton = New System.Windows.Forms.Button()
        Me.CloseButton = New System.Windows.Forms.Button()
        Me.SearchButton = New System.Windows.Forms.Button()
        Me.searchBox = New System.Windows.Forms.ComboBox()
        Me.valueText = New System.Windows.Forms.TextBox()
        Me.NoEmpVideCheckBox = New System.Windows.Forms.CheckBox()
        Me.SommeilCheckBox = New System.Windows.Forms.CheckBox()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.statusBar = New System.Windows.Forms.Label()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.retryDiscoverLabel = New System.Windows.Forms.LinkLabel()
        Me.FormatsLabel = New System.Windows.Forms.LinkLabel()
        Me.DataGridView2 = New System.Windows.Forms.DataGridView()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.RecherchePanel = New System.Windows.Forms.Panel()
        Me.CalibrateButton = New System.Windows.Forms.Button()
        Me.SuiviStockCheckBox = New System.Windows.Forms.CheckBox()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.ResultatLabel = New System.Windows.Forms.Label()
        Me.SelectionLabel = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.sendToPrint = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.EmpList = New System.Windows.Forms.ListBox()
        Me.EmpSelectList = New System.Windows.Forms.ListBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.EmpUnselectBtn = New System.Windows.Forms.Button()
        Me.EmpSelectBtn = New System.Windows.Forms.Button()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.EmpFilterBtn = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.EmpFilterText = New System.Windows.Forms.TextBox()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.EmpPrintBtn = New System.Windows.Forms.Button()
        Me.StockPositifCheckBox = New System.Windows.Forms.CheckBox()
        CType(Me.DataGridView2,System.ComponentModel.ISupportInitialize).BeginInit
        Me.TableLayoutPanel1.SuspendLayout
        Me.RecherchePanel.SuspendLayout
        CType(Me.DataGridView1,System.ComponentModel.ISupportInitialize).BeginInit
        Me.Panel2.SuspendLayout
        Me.TabControl1.SuspendLayout
        Me.TabPage1.SuspendLayout
        Me.TabPage2.SuspendLayout
        Me.TableLayoutPanel2.SuspendLayout
        Me.Panel1.SuspendLayout
        Me.Panel3.SuspendLayout
        Me.Panel4.SuspendLayout
        Me.SuspendLayout
        '
        'PrintButton
        '
        Me.PrintButton.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PrintButton.Enabled = false
        Me.PrintButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.PrintButton.Location = New System.Drawing.Point(3, 684)
        Me.PrintButton.Name = "PrintButton"
        Me.PrintButton.Size = New System.Drawing.Size(941, 34)
        Me.PrintButton.TabIndex = 1
        Me.PrintButton.Text = "Imprimer"
        Me.PrintButton.UseVisualStyleBackColor = true
        '
        'CloseButton
        '
        Me.CloseButton.Location = New System.Drawing.Point(829, 40)
        Me.CloseButton.Name = "CloseButton"
        Me.CloseButton.Size = New System.Drawing.Size(75, 23)
        Me.CloseButton.TabIndex = 3
        Me.CloseButton.Text = "Fermer"
        Me.CloseButton.UseVisualStyleBackColor = true
        '
        'SearchButton
        '
        Me.SearchButton.Location = New System.Drawing.Point(14, 72)
        Me.SearchButton.Name = "SearchButton"
        Me.SearchButton.Size = New System.Drawing.Size(75, 23)
        Me.SearchButton.TabIndex = 7
        Me.SearchButton.Text = "Chercher"
        Me.SearchButton.UseVisualStyleBackColor = true
        '
        'searchBox
        '
        Me.searchBox.AutoCompleteCustomSource.AddRange(New String() {"Emplacement commence par", "Référence Article commence par", "Numéro de fournisseur égal"})
        Me.searchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.searchBox.FormattingEnabled = true
        Me.searchBox.Location = New System.Drawing.Point(12, 43)
        Me.searchBox.Name = "searchBox"
        Me.searchBox.Size = New System.Drawing.Size(229, 21)
        Me.searchBox.TabIndex = 12
        '
        'valueText
        '
        Me.valueText.Location = New System.Drawing.Point(257, 43)
        Me.valueText.Name = "valueText"
        Me.valueText.Size = New System.Drawing.Size(219, 20)
        Me.valueText.TabIndex = 13
        '
        'NoEmpVideCheckBox
        '
        Me.NoEmpVideCheckBox.AutoSize = true
        Me.NoEmpVideCheckBox.Location = New System.Drawing.Point(495, 67)
        Me.NoEmpVideCheckBox.Name = "NoEmpVideCheckBox"
        Me.NoEmpVideCheckBox.Size = New System.Drawing.Size(237, 17)
        Me.NoEmpVideCheckBox.TabIndex = 14
        Me.NoEmpVideCheckBox.Text = "Ne pas inclure les articles sans emplacement"
        Me.NoEmpVideCheckBox.UseVisualStyleBackColor = true
        '
        'SommeilCheckBox
        '
        Me.SommeilCheckBox.AutoSize = true
        Me.SommeilCheckBox.Location = New System.Drawing.Point(495, 91)
        Me.SommeilCheckBox.Name = "SommeilCheckBox"
        Me.SommeilCheckBox.Size = New System.Drawing.Size(201, 17)
        Me.SommeilCheckBox.TabIndex = 15
        Me.SommeilCheckBox.Text = "Ne pas inclure les articles en sommeil"
        Me.SommeilCheckBox.UseVisualStyleBackColor = true
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = true
        Me.LinkLabel1.Location = New System.Drawing.Point(725, 9)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(79, 13)
        Me.LinkLabel1.TabIndex = 16
        Me.LinkLabel1.TabStop = true
        Me.LinkLabel1.Text = "Documentation"
        Me.LinkLabel1.Visible = false
        '
        'statusBar
        '
        Me.statusBar.BackColor = System.Drawing.Color.Red
        Me.statusBar.Cursor = System.Windows.Forms.Cursors.Default
        Me.statusBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.statusBar.Location = New System.Drawing.Point(0, 0)
        Me.statusBar.Name = "statusBar"
        Me.statusBar.Padding = New System.Windows.Forms.Padding(5)
        Me.statusBar.Size = New System.Drawing.Size(941, 31)
        Me.statusBar.TabIndex = 18
        Me.statusBar.Text = "Status imprimante: Non connectée"
        Me.statusBar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(61, 4)
        '
        'retryDiscoverLabel
        '
        Me.retryDiscoverLabel.AutoSize = true
        Me.retryDiscoverLabel.BackColor = System.Drawing.Color.Red
        Me.retryDiscoverLabel.Location = New System.Drawing.Point(639, 9)
        Me.retryDiscoverLabel.Name = "retryDiscoverLabel"
        Me.retryDiscoverLabel.Size = New System.Drawing.Size(57, 13)
        Me.retryDiscoverLabel.TabIndex = 19
        Me.retryDiscoverLabel.TabStop = true
        Me.retryDiscoverLabel.Text = "Réessayer"
        Me.retryDiscoverLabel.Visible = false
        '
        'FormatsLabel
        '
        Me.FormatsLabel.AutoSize = true
        Me.FormatsLabel.Location = New System.Drawing.Point(361, 91)
        Me.FormatsLabel.Name = "FormatsLabel"
        Me.FormatsLabel.Size = New System.Drawing.Size(44, 13)
        Me.FormatsLabel.TabIndex = 20
        Me.FormatsLabel.TabStop = true
        Me.FormatsLabel.Text = "Formats"
        '
        'DataGridView2
        '
        Me.DataGridView2.AllowUserToAddRows = false
        Me.DataGridView2.AllowUserToOrderColumns = true
        Me.DataGridView2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.DataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DataGridView2.Location = New System.Drawing.Point(3, 446)
        Me.DataGridView2.Name = "DataGridView2"
        Me.DataGridView2.Size = New System.Drawing.Size(941, 232)
        Me.DataGridView2.TabIndex = 8
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100!))
        Me.TableLayoutPanel1.Controls.Add(Me.RecherchePanel, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.DataGridView1, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.ResultatLabel, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.DataGridView2, 0, 5)
        Me.TableLayoutPanel1.Controls.Add(Me.PrintButton, 0, 6)
        Me.TableLayoutPanel1.Controls.Add(Me.SelectionLabel, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel2, 0, 3)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 3)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 7
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(947, 721)
        Me.TableLayoutPanel1.TabIndex = 23
        '
        'RecherchePanel
        '
        Me.RecherchePanel.Controls.Add(Me.StockPositifCheckBox)
        Me.RecherchePanel.Controls.Add(Me.retryDiscoverLabel)
        Me.RecherchePanel.Controls.Add(Me.statusBar)
        Me.RecherchePanel.Controls.Add(Me.FormatsLabel)
        Me.RecherchePanel.Controls.Add(Me.SommeilCheckBox)
        Me.RecherchePanel.Controls.Add(Me.LinkLabel1)
        Me.RecherchePanel.Controls.Add(Me.NoEmpVideCheckBox)
        Me.RecherchePanel.Controls.Add(Me.CalibrateButton)
        Me.RecherchePanel.Controls.Add(Me.searchBox)
        Me.RecherchePanel.Controls.Add(Me.SearchButton)
        Me.RecherchePanel.Controls.Add(Me.SuiviStockCheckBox)
        Me.RecherchePanel.Controls.Add(Me.CloseButton)
        Me.RecherchePanel.Controls.Add(Me.valueText)
        Me.RecherchePanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RecherchePanel.Location = New System.Drawing.Point(3, 3)
        Me.RecherchePanel.Name = "RecherchePanel"
        Me.RecherchePanel.Size = New System.Drawing.Size(941, 119)
        Me.RecherchePanel.TabIndex = 27
        '
        'CalibrateButton
        '
        Me.CalibrateButton.Location = New System.Drawing.Point(778, 87)
        Me.CalibrateButton.Name = "CalibrateButton"
        Me.CalibrateButton.Size = New System.Drawing.Size(138, 23)
        Me.CalibrateButton.TabIndex = 26
        Me.CalibrateButton.Text = "Calibrer l'imprimante"
        Me.CalibrateButton.UseVisualStyleBackColor = true
        '
        'SuiviStockCheckBox
        '
        Me.SuiviStockCheckBox.AutoSize = true
        Me.SuiviStockCheckBox.Location = New System.Drawing.Point(495, 45)
        Me.SuiviStockCheckBox.Name = "SuiviStockCheckBox"
        Me.SuiviStockCheckBox.Size = New System.Drawing.Size(93, 17)
        Me.SuiviStockCheckBox.TabIndex = 25
        Me.SuiviStockCheckBox.Text = "Suivi en stock"
        Me.SuiviStockCheckBox.UseVisualStyleBackColor = true
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = false
        Me.DataGridView1.AllowUserToOrderColumns = true
        Me.DataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DataGridView1.Location = New System.Drawing.Point(3, 148)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.Size = New System.Drawing.Size(941, 232)
        Me.DataGridView1.TabIndex = 8
        '
        'ResultatLabel
        '
        Me.ResultatLabel.AutoSize = true
        Me.ResultatLabel.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.ResultatLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ResultatLabel.Location = New System.Drawing.Point(3, 125)
        Me.ResultatLabel.Name = "ResultatLabel"
        Me.ResultatLabel.Size = New System.Drawing.Size(941, 20)
        Me.ResultatLabel.TabIndex = 10
        Me.ResultatLabel.Text = "Résultat de la recherche"
        Me.ResultatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'SelectionLabel
        '
        Me.SelectionLabel.AutoSize = true
        Me.SelectionLabel.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.SelectionLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SelectionLabel.Location = New System.Drawing.Point(3, 423)
        Me.SelectionLabel.Name = "SelectionLabel"
        Me.SelectionLabel.Size = New System.Drawing.Size(941, 20)
        Me.SelectionLabel.TabIndex = 11
        Me.SelectionLabel.Text = "Fenêtre de sélection"
        Me.SelectionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.sendToPrint)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(3, 386)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(941, 34)
        Me.Panel2.TabIndex = 12
        '
        'sendToPrint
        '
        Me.sendToPrint.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sendToPrint.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.sendToPrint.Location = New System.Drawing.Point(0, 0)
        Me.sendToPrint.Name = "sendToPrint"
        Me.sendToPrint.Size = New System.Drawing.Size(941, 34)
        Me.sendToPrint.TabIndex = 0
        Me.sendToPrint.Text = "Ajouter à la selection"
        Me.sendToPrint.UseVisualStyleBackColor = true
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControl1.Location = New System.Drawing.Point(0, 0)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(961, 753)
        Me.TabControl1.TabIndex = 27
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.TableLayoutPanel1)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(953, 727)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Articles"
        Me.TabPage1.UseVisualStyleBackColor = true
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.TableLayoutPanel2)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(953, 727)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Emplacements"
        Me.TabPage2.UseVisualStyleBackColor = true
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 4
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 189!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 467!))
        Me.TableLayoutPanel2.Controls.Add(Me.EmpList, 0, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.EmpSelectList, 2, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.Panel1, 1, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.Panel3, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Panel4, 2, 0)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(3, 3)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 2
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 145!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(947, 721)
        Me.TableLayoutPanel2.TabIndex = 0
        '
        'EmpList
        '
        Me.EmpList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.EmpList.FormattingEnabled = true
        Me.EmpList.Location = New System.Drawing.Point(3, 95)
        Me.EmpList.Name = "EmpList"
        Me.EmpList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.EmpList.Size = New System.Drawing.Size(180, 623)
        Me.EmpList.TabIndex = 0
        '
        'EmpSelectList
        '
        Me.EmpSelectList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.EmpSelectList.FormattingEnabled = true
        Me.EmpSelectList.Location = New System.Drawing.Point(294, 95)
        Me.EmpSelectList.Name = "EmpSelectList"
        Me.EmpSelectList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.EmpSelectList.Size = New System.Drawing.Size(183, 623)
        Me.EmpSelectList.TabIndex = 1
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.EmpUnselectBtn)
        Me.Panel1.Controls.Add(Me.EmpSelectBtn)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(189, 95)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(99, 623)
        Me.Panel1.TabIndex = 2
        '
        'EmpUnselectBtn
        '
        Me.EmpUnselectBtn.Location = New System.Drawing.Point(12, 281)
        Me.EmpUnselectBtn.Name = "EmpUnselectBtn"
        Me.EmpUnselectBtn.Size = New System.Drawing.Size(75, 23)
        Me.EmpUnselectBtn.TabIndex = 1
        Me.EmpUnselectBtn.Text = "<<"
        Me.EmpUnselectBtn.UseVisualStyleBackColor = true
        '
        'EmpSelectBtn
        '
        Me.EmpSelectBtn.Location = New System.Drawing.Point(12, 242)
        Me.EmpSelectBtn.Name = "EmpSelectBtn"
        Me.EmpSelectBtn.Size = New System.Drawing.Size(75, 23)
        Me.EmpSelectBtn.TabIndex = 0
        Me.EmpSelectBtn.Text = ">>"
        Me.EmpSelectBtn.UseVisualStyleBackColor = true
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.EmpFilterBtn)
        Me.Panel3.Controls.Add(Me.Label1)
        Me.Panel3.Controls.Add(Me.EmpFilterText)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel3.Location = New System.Drawing.Point(3, 3)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(180, 86)
        Me.Panel3.TabIndex = 3
        '
        'EmpFilterBtn
        '
        Me.EmpFilterBtn.Location = New System.Drawing.Point(133, 36)
        Me.EmpFilterBtn.Name = "EmpFilterBtn"
        Me.EmpFilterBtn.Size = New System.Drawing.Size(31, 23)
        Me.EmpFilterBtn.TabIndex = 2
        Me.EmpFilterBtn.Text = "Go"
        Me.EmpFilterBtn.UseVisualStyleBackColor = true
        '
        'Label1
        '
        Me.Label1.AutoSize = true
        Me.Label1.Location = New System.Drawing.Point(61, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(29, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Filtre"
        '
        'EmpFilterText
        '
        Me.EmpFilterText.Location = New System.Drawing.Point(6, 38)
        Me.EmpFilterText.Name = "EmpFilterText"
        Me.EmpFilterText.Size = New System.Drawing.Size(111, 20)
        Me.EmpFilterText.TabIndex = 0
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.EmpPrintBtn)
        Me.Panel4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel4.Location = New System.Drawing.Point(294, 3)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(183, 86)
        Me.Panel4.TabIndex = 4
        '
        'EmpPrintBtn
        '
        Me.EmpPrintBtn.Location = New System.Drawing.Point(50, 35)
        Me.EmpPrintBtn.Name = "EmpPrintBtn"
        Me.EmpPrintBtn.Size = New System.Drawing.Size(75, 23)
        Me.EmpPrintBtn.TabIndex = 0
        Me.EmpPrintBtn.Text = "Imprimer"
        Me.EmpPrintBtn.UseVisualStyleBackColor = true
        '
        'StockPositifCheckBox
        '
        Me.StockPositifCheckBox.AutoSize = true
        Me.StockPositifCheckBox.Location = New System.Drawing.Point(594, 45)
        Me.StockPositifCheckBox.Name = "StockPositifCheckBox"
        Me.StockPositifCheckBox.Size = New System.Drawing.Size(84, 17)
        Me.StockPositifCheckBox.TabIndex = 27
        Me.StockPositifCheckBox.Text = "Stock positif"
        Me.StockPositifCheckBox.UseVisualStyleBackColor = true
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(961, 753)
        Me.Controls.Add(Me.TabControl1)
        Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
        Me.Name = "Form1"
        Me.Text = "Etiquettes"
        CType(Me.DataGridView2,System.ComponentModel.ISupportInitialize).EndInit
        Me.TableLayoutPanel1.ResumeLayout(false)
        Me.TableLayoutPanel1.PerformLayout
        Me.RecherchePanel.ResumeLayout(false)
        Me.RecherchePanel.PerformLayout
        CType(Me.DataGridView1,System.ComponentModel.ISupportInitialize).EndInit
        Me.Panel2.ResumeLayout(false)
        Me.TabControl1.ResumeLayout(false)
        Me.TabPage1.ResumeLayout(false)
        Me.TabPage2.ResumeLayout(false)
        Me.TableLayoutPanel2.ResumeLayout(false)
        Me.Panel1.ResumeLayout(false)
        Me.Panel3.ResumeLayout(false)
        Me.Panel3.PerformLayout
        Me.Panel4.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents PrintButton As System.Windows.Forms.Button
    Friend WithEvents CloseButton As System.Windows.Forms.Button
    Friend WithEvents SearchButton As System.Windows.Forms.Button
    Friend WithEvents valueText As System.Windows.Forms.TextBox
    Friend WithEvents NoEmpVideCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents searchBox As System.Windows.Forms.ComboBox
    Friend WithEvents SommeilCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Friend WithEvents statusBar As System.Windows.Forms.Label
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents retryDiscoverLabel As System.Windows.Forms.LinkLabel
    Friend WithEvents FormatsLabel As System.Windows.Forms.LinkLabel
    Friend WithEvents DataGridView2 As System.Windows.Forms.DataGridView
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents ResultatLabel As System.Windows.Forms.Label
    Friend WithEvents sendToPrint As System.Windows.Forms.Button
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents SelectionLabel As System.Windows.Forms.Label
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents SuiviStockCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents CalibrateButton As System.Windows.Forms.Button
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents RecherchePanel As Panel
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents EmpList As ListBox
    Friend WithEvents EmpSelectList As ListBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents EmpUnselectBtn As Button
    Friend WithEvents EmpSelectBtn As Button
    Friend WithEvents Panel3 As Panel
    Friend WithEvents EmpFilterBtn As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents EmpFilterText As TextBox
    Friend WithEvents Panel4 As Panel
    Friend WithEvents EmpPrintBtn As Button
    Friend WithEvents StockPositifCheckBox As CheckBox
End Class
