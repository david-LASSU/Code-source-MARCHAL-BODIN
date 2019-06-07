Imports System.Drawing.Printing
Public Class PrintPreviewDialogSelectPrinter
    Inherits PrintPreviewDialog

    Friend WithEvents PrintDialog1 As PrintDialog

    Private Sub myPrintPreview_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        'Get the toolstrip from the base control
        Dim ts As ToolStrip = CType(Me.Controls(1), ToolStrip)
        'Get the print button from the toolstrip
        Dim printItem As ToolStripItem = ts.Items("printToolStripButton")

        'Add a new button 
        With printItem

            Dim myPrintItem As ToolStripItem
            myPrintItem = ts.Items.Add(.Text, .Image, New EventHandler(AddressOf MyPrintItemClicked))

            myPrintItem.DisplayStyle = ToolStripItemDisplayStyle.Image
            'Relocate the item to the beginning of the toolstrip
            ts.Items.Insert(0, myPrintItem)
        End With

        'Remove the orginal button
        ts.Items.Remove(printItem)
    End Sub

    Public Event MyPrintItemClick()

    Private Sub MyPrintItemClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim dlgPrint As New PrintDialog

        Try
            With dlgPrint
                .UseEXDialog = True
                .AllowSelection = True
                .ShowNetwork = True
                .Document = Me.Document
            End With

            If dlgPrint.ShowDialog() = DialogResult.OK Then
                RaiseEvent MyPrintItemClick()
                Me.Document.Print()
            End If
        Catch ex As Exception
            MsgBox("Print Error: " & ex.Message)
        End Try
    End Sub

    Private Sub InitializeComponent()
        Me.PrintDialog1 = New System.Windows.Forms.PrintDialog()
        Me.SuspendLayout()
        '
        'PrintDialog1
        '
        Me.PrintDialog1.UseEXDialog = True
        '
        'PrintPreviewDialogSelectPrinter
        '
        Me.ClientSize = New System.Drawing.Size(400, 300)
        Me.Name = "PrintPreviewDialogSelectPrinter"
        Me.ResumeLayout(False)

    End Sub
End Class
