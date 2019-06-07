Imports MBCore.Model

Public Class Form1
    Private fournRepos As FournRepository

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If Environment.GetCommandLineArgs.Count < 1 Then
                Throw New Exception("Nombre d'arguments insuffisant pour démarrer")
            End If

            Dim fichierGescom As String = Environment.GetCommandLineArgs.GetValue(1)
            BaseCialAbstract.setDefaultParams(fichierGescom)
            fournRepos = New FournRepository
            ActiveControl = TextBox1
        Catch ex As Exception
            MsgBox(String.Format("Connection Serveur Impossible : {0}", ex.Message))
            Application.Exit()
        End Try
    End Sub

    ' Liste en fonction des paramètres
    Private Sub listFourns()
        Dim value As String = TextBox1.Text
        DataGridView1.DataSource = Nothing

        Dim dt As DataTable = fournRepos.GetFourns(TextBox1.Text, CheckBoxActifs.Checked, CheckBoxSommeil.Checked)

        If dt.Rows.Count = 0 Then
            MsgBox("Aucun fournisseur n'a été trouvé")
        Else
            DataGridView1.DataSource = dt
            With DataGridView1
                .Columns("CT_Num").HeaderCell.Value = "Numéro"
                .Columns("CT_Intitule").HeaderCell.Value = "Intitulé"
                .Columns("DATE_MAJ_TARIF").HeaderCell.Value = "Date Maj"
                .Columns("NB_CBVIDES").HeaderCell.Value = "Codes barres vides"
                .Columns("NB_ART").HeaderCell.Value = "Nombre d'Articles"
                .Refresh()
            End With
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        listFourns()
    End Sub

    Private Sub DataGridView1_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles DataGridView1.CellFormatting
        If Not IsDBNull(DataGridView1.Rows(e.RowIndex).Cells(2).Value) Then
            Dim year As Date = DataGridView1.Rows(e.RowIndex).Cells(2).Value
            Dim bgColor As Color

            Select Case year.Year
                Case Is = Date.Now.Year
                    bgColor = Color.YellowGreen
                Case Is = Date.Now.Year - 1
                    bgColor = Color.Yellow
                Case Else
                    bgColor = Color.Orange
            End Select
            DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = bgColor
        End If
        If Not IsDBNull(DataGridView1.Rows(e.RowIndex).Cells(4).Value) Then
            If DataGridView1.Rows(e.RowIndex).Cells(4).Value = "oui" Then
                DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.Red
            End If
        End If
        If DataGridView1.Rows(e.RowIndex).Cells(5).Value <> "" Then
            DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = Color.Aqua
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Application.Exit()
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            listFourns()
        End If
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        ColorsLegend.Show()
    End Sub
End Class
