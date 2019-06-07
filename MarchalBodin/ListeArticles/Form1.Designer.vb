<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.RefComboBox = New System.Windows.Forms.ComboBox()
        Me.RefTextBox1 = New System.Windows.Forms.TextBox()
        Me.RefTextBox2 = New System.Windows.Forms.TextBox()
        Me.RefEtLabel = New System.Windows.Forms.Label()
        Me.RefLabel = New System.Windows.Forms.Label()
        Me.DesignLabel = New System.Windows.Forms.Label()
        Me.DesignTextBox = New System.Windows.Forms.TextBox()
        Me.FournTextBox = New System.Windows.Forms.TextBox()
        Me.ArtStatusComboBox = New System.Windows.Forms.ComboBox()
        Me.SearchButton = New System.Windows.Forms.Button()
        Me.NbResultsLabel = New System.Windows.Forms.Label()
        Me.FournLabel = New System.Windows.Forms.LinkLabel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.CodeBarreTextBox = New System.Windows.Forms.TextBox()
        Me.codeBarreLabel = New System.Windows.Forms.Label()
        Me.EmplacementLabel = New System.Windows.Forms.Label()
        Me.EmplacementTextBox = New System.Windows.Forms.TextBox()
        Me.SuiviCheckBox = New System.Windows.Forms.CheckBox()
        Me.ResetButton = New System.Windows.Forms.Button()
        Me.FournPrincComboBox = New System.Windows.Forms.ComboBox()
        Me.SuppUsineCheckBox = New System.Windows.Forms.CheckBox()
        Me.SuppCheckBox = New System.Windows.Forms.CheckBox()
        Me.FournListBox = New System.Windows.Forms.ListBox()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.BottomToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.TopToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.RightToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.LeftToolStripPanel = New System.Windows.Forms.ToolStripPanel()
        Me.ContentPanel = New System.Windows.Forms.ToolStripContentPanel()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.Panel1.SuspendLayout
        Me.TableLayoutPanel1.SuspendLayout
        CType(Me.DataGridView1,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.SplitContainer1,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SplitContainer1.Panel1.SuspendLayout
        Me.SplitContainer1.Panel2.SuspendLayout
        Me.SplitContainer1.SuspendLayout
        Me.SuspendLayout
        '
        'RefComboBox
        '
        Me.RefComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.RefComboBox.FormattingEnabled = true
        Me.RefComboBox.Items.AddRange(New Object() {"tous", "contient", "est compris entre", "commence par", "se termine par", "est égal à", "est différent de", "est supérieur ou égal à", "est supérieur à", "est inférieur ou égal à", "est inférieur à"})
        Me.RefComboBox.Location = New System.Drawing.Point(89, 44)
        Me.RefComboBox.Name = "RefComboBox"
        Me.RefComboBox.Size = New System.Drawing.Size(156, 21)
        Me.RefComboBox.TabIndex = 0
        '
        'RefTextBox1
        '
        Me.RefTextBox1.Location = New System.Drawing.Point(266, 44)
        Me.RefTextBox1.Name = "RefTextBox1"
        Me.RefTextBox1.Size = New System.Drawing.Size(132, 20)
        Me.RefTextBox1.TabIndex = 1
        '
        'RefTextBox2
        '
        Me.RefTextBox2.Location = New System.Drawing.Point(433, 44)
        Me.RefTextBox2.Name = "RefTextBox2"
        Me.RefTextBox2.Size = New System.Drawing.Size(137, 20)
        Me.RefTextBox2.TabIndex = 2
        '
        'RefEtLabel
        '
        Me.RefEtLabel.AutoSize = true
        Me.RefEtLabel.Location = New System.Drawing.Point(411, 47)
        Me.RefEtLabel.Name = "RefEtLabel"
        Me.RefEtLabel.Size = New System.Drawing.Size(16, 13)
        Me.RefEtLabel.TabIndex = 3
        Me.RefEtLabel.Text = "et"
        '
        'RefLabel
        '
        Me.RefLabel.AutoSize = true
        Me.RefLabel.Location = New System.Drawing.Point(11, 47)
        Me.RefLabel.Name = "RefLabel"
        Me.RefLabel.Size = New System.Drawing.Size(57, 13)
        Me.RefLabel.TabIndex = 4
        Me.RefLabel.Text = "Référence"
        '
        'DesignLabel
        '
        Me.DesignLabel.AutoSize = true
        Me.DesignLabel.Location = New System.Drawing.Point(3, 18)
        Me.DesignLabel.Name = "DesignLabel"
        Me.DesignLabel.Size = New System.Drawing.Size(114, 13)
        Me.DesignLabel.TabIndex = 5
        Me.DesignLabel.Text = "Désignation, référence"
        '
        'DesignTextBox
        '
        Me.DesignTextBox.Location = New System.Drawing.Point(123, 18)
        Me.DesignTextBox.Name = "DesignTextBox"
        Me.DesignTextBox.Size = New System.Drawing.Size(382, 20)
        Me.DesignTextBox.TabIndex = 6
        '
        'FournTextBox
        '
        Me.FournTextBox.Location = New System.Drawing.Point(89, 79)
        Me.FournTextBox.Name = "FournTextBox"
        Me.FournTextBox.Size = New System.Drawing.Size(481, 20)
        Me.FournTextBox.TabIndex = 8
        '
        'ArtStatusComboBox
        '
        Me.ArtStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ArtStatusComboBox.FormattingEnabled = true
        Me.ArtStatusComboBox.Items.AddRange(New Object() {"Actif", "Sommeil", "Tous"})
        Me.ArtStatusComboBox.Location = New System.Drawing.Point(576, 43)
        Me.ArtStatusComboBox.Name = "ArtStatusComboBox"
        Me.ArtStatusComboBox.Size = New System.Drawing.Size(104, 21)
        Me.ArtStatusComboBox.TabIndex = 10
        '
        'SearchButton
        '
        Me.SearchButton.Location = New System.Drawing.Point(686, 75)
        Me.SearchButton.Name = "SearchButton"
        Me.SearchButton.Size = New System.Drawing.Size(83, 23)
        Me.SearchButton.TabIndex = 12
        Me.SearchButton.Text = "Rechercher"
        Me.SearchButton.UseVisualStyleBackColor = true
        '
        'NbResultsLabel
        '
        Me.NbResultsLabel.AutoSize = true
        Me.NbResultsLabel.Location = New System.Drawing.Point(698, 109)
        Me.NbResultsLabel.Name = "NbResultsLabel"
        Me.NbResultsLabel.Size = New System.Drawing.Size(82, 13)
        Me.NbResultsLabel.TabIndex = 13
        Me.NbResultsLabel.Text = "NbResultsLabel"
        Me.NbResultsLabel.Visible = false
        '
        'FournLabel
        '
        Me.FournLabel.AutoSize = true
        Me.FournLabel.Enabled = false
        Me.FournLabel.Location = New System.Drawing.Point(11, 82)
        Me.FournLabel.Name = "FournLabel"
        Me.FournLabel.Size = New System.Drawing.Size(61, 13)
        Me.FournLabel.TabIndex = 14
        Me.FournLabel.TabStop = true
        Me.FournLabel.Text = "Fournisseur"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.CodeBarreTextBox)
        Me.Panel1.Controls.Add(Me.codeBarreLabel)
        Me.Panel1.Controls.Add(Me.EmplacementLabel)
        Me.Panel1.Controls.Add(Me.EmplacementTextBox)
        Me.Panel1.Controls.Add(Me.SuiviCheckBox)
        Me.Panel1.Controls.Add(Me.ResetButton)
        Me.Panel1.Controls.Add(Me.FournPrincComboBox)
        Me.Panel1.Controls.Add(Me.SuppUsineCheckBox)
        Me.Panel1.Controls.Add(Me.SuppCheckBox)
        Me.Panel1.Controls.Add(Me.FournLabel)
        Me.Panel1.Controls.Add(Me.NbResultsLabel)
        Me.Panel1.Controls.Add(Me.SearchButton)
        Me.Panel1.Controls.Add(Me.ArtStatusComboBox)
        Me.Panel1.Controls.Add(Me.FournTextBox)
        Me.Panel1.Controls.Add(Me.DesignTextBox)
        Me.Panel1.Controls.Add(Me.DesignLabel)
        Me.Panel1.Controls.Add(Me.RefLabel)
        Me.Panel1.Controls.Add(Me.RefEtLabel)
        Me.Panel1.Controls.Add(Me.RefTextBox2)
        Me.Panel1.Controls.Add(Me.RefTextBox1)
        Me.Panel1.Controls.Add(Me.RefComboBox)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(3, 3)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1065, 134)
        Me.Panel1.TabIndex = 0
        '
        'CodeBarreTextBox
        '
        Me.CodeBarreTextBox.Location = New System.Drawing.Point(576, 18)
        Me.CodeBarreTextBox.Name = "CodeBarreTextBox"
        Me.CodeBarreTextBox.Size = New System.Drawing.Size(137, 20)
        Me.CodeBarreTextBox.TabIndex = 25
        '
        'codeBarreLabel
        '
        Me.codeBarreLabel.AutoSize = true
        Me.codeBarreLabel.Location = New System.Drawing.Point(511, 21)
        Me.codeBarreLabel.Name = "codeBarreLabel"
        Me.codeBarreLabel.Size = New System.Drawing.Size(59, 13)
        Me.codeBarreLabel.TabIndex = 24
        Me.codeBarreLabel.Text = "Code barre"
        '
        'EmplacementLabel
        '
        Me.EmplacementLabel.AutoSize = true
        Me.EmplacementLabel.Location = New System.Drawing.Point(719, 21)
        Me.EmplacementLabel.Name = "EmplacementLabel"
        Me.EmplacementLabel.Size = New System.Drawing.Size(144, 13)
        Me.EmplacementLabel.TabIndex = 23
        Me.EmplacementLabel.Text = "Emplacement commence par"
        '
        'EmplacementTextBox
        '
        Me.EmplacementTextBox.Location = New System.Drawing.Point(869, 18)
        Me.EmplacementTextBox.Name = "EmplacementTextBox"
        Me.EmplacementTextBox.Size = New System.Drawing.Size(94, 20)
        Me.EmplacementTextBox.TabIndex = 22
        '
        'SuiviCheckBox
        '
        Me.SuiviCheckBox.AutoSize = true
        Me.SuiviCheckBox.Location = New System.Drawing.Point(871, 47)
        Me.SuiviCheckBox.Name = "SuiviCheckBox"
        Me.SuiviCheckBox.Size = New System.Drawing.Size(95, 17)
        Me.SuiviCheckBox.TabIndex = 21
        Me.SuiviCheckBox.Text = "Suivi en Stock"
        Me.SuiviCheckBox.UseVisualStyleBackColor = true
        '
        'ResetButton
        '
        Me.ResetButton.Location = New System.Drawing.Point(776, 75)
        Me.ResetButton.Name = "ResetButton"
        Me.ResetButton.Size = New System.Drawing.Size(83, 23)
        Me.ResetButton.TabIndex = 20
        Me.ResetButton.Text = "Reset"
        Me.ResetButton.UseVisualStyleBackColor = true
        '
        'FournPrincComboBox
        '
        Me.FournPrincComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.FournPrincComboBox.Enabled = false
        Me.FournPrincComboBox.FormattingEnabled = true
        Me.FournPrincComboBox.Items.AddRange(New Object() {"Principal", "Secondaire", "Tous"})
        Me.FournPrincComboBox.Location = New System.Drawing.Point(577, 77)
        Me.FournPrincComboBox.Name = "FournPrincComboBox"
        Me.FournPrincComboBox.Size = New System.Drawing.Size(103, 21)
        Me.FournPrincComboBox.TabIndex = 19
        '
        'SuppUsineCheckBox
        '
        Me.SuppUsineCheckBox.AutoSize = true
        Me.SuppUsineCheckBox.Location = New System.Drawing.Point(766, 47)
        Me.SuppUsineCheckBox.Name = "SuppUsineCheckBox"
        Me.SuppUsineCheckBox.Size = New System.Drawing.Size(98, 17)
        Me.SuppUsineCheckBox.TabIndex = 18
        Me.SuppUsineCheckBox.Text = "Supprimé usine"
        Me.SuppUsineCheckBox.UseVisualStyleBackColor = true
        '
        'SuppCheckBox
        '
        Me.SuppCheckBox.AutoSize = true
        Me.SuppCheckBox.Location = New System.Drawing.Point(686, 46)
        Me.SuppCheckBox.Name = "SuppCheckBox"
        Me.SuppCheckBox.Size = New System.Drawing.Size(70, 17)
        Me.SuppCheckBox.TabIndex = 17
        Me.SuppCheckBox.Text = "Supprimé"
        Me.SuppCheckBox.UseVisualStyleBackColor = true
        '
        'FournListBox
        '
        Me.FournListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.FournListBox.FormattingEnabled = true
        Me.FournListBox.Location = New System.Drawing.Point(34, 506)
        Me.FournListBox.Name = "FournListBox"
        Me.FournListBox.Size = New System.Drawing.Size(481, 82)
        Me.FournListBox.TabIndex = 9
        Me.FournListBox.Visible = false
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100!))
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.DataGridView1, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 140!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1071, 613)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = false
        Me.DataGridView1.AllowUserToDeleteRows = false
        Me.DataGridView1.AllowUserToOrderColumns = true
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight
        Me.DataGridView1.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DataGridView1.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.DataGridView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DataGridView1.Location = New System.Drawing.Point(3, 143)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.DataGridView1.Size = New System.Drawing.Size(1065, 467)
        Me.DataGridView1.TabIndex = 1
        Me.DataGridView1.VirtualMode = true
        '
        'BottomToolStripPanel
        '
        Me.BottomToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.BottomToolStripPanel.Name = "BottomToolStripPanel"
        Me.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.BottomToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.BottomToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'TopToolStripPanel
        '
        Me.TopToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.TopToolStripPanel.Name = "TopToolStripPanel"
        Me.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.TopToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.TopToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'RightToolStripPanel
        '
        Me.RightToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.RightToolStripPanel.Name = "RightToolStripPanel"
        Me.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.RightToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.RightToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'LeftToolStripPanel
        '
        Me.LeftToolStripPanel.Location = New System.Drawing.Point(0, 0)
        Me.LeftToolStripPanel.Name = "LeftToolStripPanel"
        Me.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.LeftToolStripPanel.RowMargin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.LeftToolStripPanel.Size = New System.Drawing.Size(0, 0)
        '
        'ContentPanel
        '
        Me.ContentPanel.Size = New System.Drawing.Size(481, 267)
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.TreeView1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.TableLayoutPanel1)
        Me.SplitContainer1.Size = New System.Drawing.Size(1334, 613)
        Me.SplitContainer1.SplitterDistance = 259
        Me.SplitContainer1.TabIndex = 10
        '
        'TreeView1
        '
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.Location = New System.Drawing.Point(0, 0)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.Size = New System.Drawing.Size(259, 613)
        Me.TreeView1.TabIndex = 0
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(61, 4)
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1334, 613)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.FournListBox)
        Me.Icon = CType(resources.GetObject("$this.Icon"),System.Drawing.Icon)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.Panel1.ResumeLayout(false)
        Me.Panel1.PerformLayout
        Me.TableLayoutPanel1.ResumeLayout(false)
        CType(Me.DataGridView1,System.ComponentModel.ISupportInitialize).EndInit
        Me.SplitContainer1.Panel1.ResumeLayout(false)
        Me.SplitContainer1.Panel2.ResumeLayout(false)
        CType(Me.SplitContainer1,System.ComponentModel.ISupportInitialize).EndInit
        Me.SplitContainer1.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents RefComboBox As ComboBox
    Friend WithEvents RefTextBox1 As TextBox
    Friend WithEvents RefTextBox2 As TextBox
    Friend WithEvents RefEtLabel As Label
    Friend WithEvents RefLabel As Label
    Friend WithEvents DesignLabel As Label
    Friend WithEvents DesignTextBox As TextBox
    Friend WithEvents FournTextBox As TextBox
    Friend WithEvents ArtStatusComboBox As ComboBox
    Friend WithEvents SearchButton As Button
    Friend WithEvents NbResultsLabel As Label
    Friend WithEvents FournLabel As LinkLabel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents FournListBox As ListBox
    Friend WithEvents BottomToolStripPanel As ToolStripPanel
    Friend WithEvents TopToolStripPanel As ToolStripPanel
    Friend WithEvents RightToolStripPanel As ToolStripPanel
    Friend WithEvents LeftToolStripPanel As ToolStripPanel
    Friend WithEvents ContentPanel As ToolStripContentPanel
    Friend WithEvents SuppUsineCheckBox As CheckBox
    Friend WithEvents SuppCheckBox As CheckBox
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents FournPrincComboBox As ComboBox
    Friend WithEvents ResetButton As Button
    Friend WithEvents SuiviCheckBox As CheckBox
    Friend WithEvents EmplacementLabel As Label
    Friend WithEvents EmplacementTextBox As TextBox
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents CodeBarreTextBox As TextBox
    Friend WithEvents codeBarreLabel As Label
End Class
