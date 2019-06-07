Public Class ColorsLegend

    Private Sub ColorsLegend_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim cl As New List(Of ColorLegend)
        cl.Add(New ColorLegend(Color.YellowGreen, "Le fournisseur a été mis à jour cette année"))
        cl.Add(New ColorLegend(Color.Yellow, "Le fournisseur a été mis à jour l'année dernière"))
        cl.Add(New ColorLegend(Color.Orange, "Le fournisseur a été mis à jour avant l'année dernière"))
        cl.Add(New ColorLegend(Color.Red, "Une mise à jour urgente a été demandée sur ce fournisseur cf. MAJ_URGENTE"))
        cl.Add(New ColorLegend(Color.Aqua, "Le fournisseur est en cours de mise à jour cf. EN_COURS_DE_MAJ"))
        cl.Add(New ColorLegend(Color.White, "Impossible d'avoir le satus de ce fournisseur"))

        Dim clBinding As New BindingSource
        clBinding.DataSource = cl

        DataGridView1.DataSource = clBinding

        DataGridView1.Columns(0).Visible = False
        DataGridView1.Columns(1).HeaderCell.Value = "Définition"
    End Sub

    Private Sub DataGridView1_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles DataGridView1.CellFormatting
        DataGridView1.Rows(e.RowIndex).DefaultCellStyle.BackColor = DataGridView1.Rows(e.RowIndex).Cells(0).Value
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub
End Class