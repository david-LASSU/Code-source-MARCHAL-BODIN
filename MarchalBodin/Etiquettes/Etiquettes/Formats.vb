Imports System.Threading

Public Class Formats
    Private waitLabel As New Label
    Delegate Sub ShowImageCallBack(ByVal picture As PictureBox, img As Image)

    Private Sub Formats_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        waitLabel.Text = "Veuillez Patienter ..."
        TableLayoutPanel1.Controls.Add(waitLabel, 0, 0)
        Dim i As Integer = 1
        For Each frm As Format In New FormatCollection

            Dim Picture As New PictureBox

            Picture.Width = frm.width / 2
            Picture.Height = frm.height / 2
            Picture.SizeMode = PictureBoxSizeMode.StretchImage

            Dim KeyLabel As New Label
            KeyLabel.Text = frm.key
            KeyLabel.TextAlign = ContentAlignment.TopLeft
            KeyLabel.Anchor = AnchorStyles.Top
            'KeyLabel.AutoSize = True

            Dim DescLabel As New Label
            DescLabel.Text = frm.description
            DescLabel.TextAlign = ContentAlignment.TopLeft
            DescLabel.Anchor = AnchorStyles.Top
            'DescLabel.AutoSize = True

            Dim TypeLabel As New Label
            TypeLabel.Text = "Imprimante: "&frm.type
            TypeLabel.TextAlign = ContentAlignment.TopLeft
            TypeLabel.Anchor = AnchorStyles.Top

            Dim p As New FlowLayoutPanel
            p.Dock = DockStyle.Fill
            p.FlowDirection = FlowDirection.TopDown
            p.Controls.Add(KeyLabel)
            p.Controls.Add(DescLabel)
            p.Controls.Add(TypeLabel)

            TableLayoutPanel1.Controls.Add(p, 0, i)
            TableLayoutPanel1.Controls.Add(Picture, 1, i)

            Dim t As New Thread(Sub() LoadImage(Picture, frm))
            t.Start()
            i += 1
        Next
    End Sub

    Private Sub LoadImage(ByVal picture As PictureBox, frm As IFormat)
        Try
            Invoke(New ShowImageCallBack(AddressOf ShowImage), picture, frm.GetPreviewImage())
        Catch ex As Exception
            ' TODO Show error message
            Debug.Print(ex.Message)
        End Try
    End Sub

    Private Sub ShowImage(ByVal picture As PictureBox, img As Image)
        picture.Image = img
        picture.Refresh()
        waitLabel.Text = ""
        Me.TableLayoutPanel1.Controls.Remove(waitLabel)
        'Me.Refresh()
    End Sub
End Class