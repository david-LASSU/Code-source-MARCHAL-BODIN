Imports Etiquettes
Imports Zen.Barcode

Public Class FormatNormal
    Inherits Format
    Implements IFormatNormal
    Public Const FORMAT_3 = "FORMAT_3"
    Public Const FORMAT_4 = "FORMAT_4"
    Public Const FORMAT_5 = "FORMAT_5"
    Public Overrides ReadOnly Property type As String
        Get
            Return "normal"
        End Get
    End Property

    Public Function DrawLabel(ByRef pLabel As PrintLabel, ByRef g As Graphics, ByRef maxWidth As Integer, ByRef maxHeight As Integer, ByRef x As Integer, ByRef y As Integer) As Boolean Implements IFormatNormal.DrawLabel
        Select Case key
            Case FORMAT_3
                Return DrawLabelF3(pLabel, g, maxWidth, maxHeight, x, y)
            Case FORMAT_4
                Return DrawLabelF4(pLabel, g, maxWidth, maxHeight, x, y)
            Case FORMAT_5
                Return DrawLabelF5(pLabel, g, maxWidth, maxHeight, x, y)
            Case Else
                Throw New Exception("Format not found")
        End Select
    End Function

    Private Function DrawLabelF3(ByRef pLabel As PrintLabel, ByRef g As Graphics, ByRef maxWidth As Integer, ByRef maxHeight As Integer, ByRef x As Integer, ByRef y As Integer) As Boolean
        Dim margin As Integer = 10
        Dim padding As Integer = 10
        Dim f12 As New Font("Arial", 12, FontStyle.Regular)
        Dim f12b As New Font("Arial", 12, FontStyle.Bold)
        Dim pen As New Pen(Color.Gray, 1) With {
            .DashPattern = {2, 2}
        }

        If x = 0 Then
            x += margin
        End If
        If y = 0 Then
            y += margin
        End If

        ' Repères de découpe
        Dim labRect As New Rectangle(x, y, pLabel.format.width, pLabel.format.height)
        g.DrawLine(pen, x, y, x + 20, y)
        g.DrawLine(pen, x, y, x, y + 20)
        g.DrawLine(pen, x + pLabel.format.width, y, x + pLabel.format.width - 20, y)
        g.DrawLine(pen, x + pLabel.format.width, y, x + pLabel.format.width, y + 20)
        g.DrawLine(pen, x, y + pLabel.format.height, x, y + pLabel.format.height - 20)
        g.DrawLine(pen, x, y + pLabel.format.height, x + 20, y + pLabel.format.height)
        g.DrawLine(pen, x + pLabel.format.width, y + pLabel.format.height, x + pLabel.format.width - 20, y + pLabel.format.height)
        g.DrawLine(pen, x + pLabel.format.width, y + pLabel.format.height, x + pLabel.format.width, y + pLabel.format.height - 20)

        ' Padding
        x += padding
        y += padding

        ' Designation
        Dim desRect As New Rectangle(x, y, pLabel.format.width, f12b.Height * 2)
        g.DrawString(pLabel.designation, f12b, Brushes.Black, desRect)
        y += desRect.Height + 5
        ' Reference
        Dim fRef As New Font("Arial", 20, FontStyle.Bold)
        g.DrawString(pLabel.refMag, fRef, Brushes.Black, x, y)
        ' Colisage
        g.DrawString(pLabel.colisage, f12, Brushes.Black, x + g.MeasureString(pLabel.refMag, fRef).Width + 30, y)
        y += g.MeasureString(pLabel.colisage, f12).Height
        ' Emplacement (si vide, remplace par un espace pour garder l'escpace hauteur
        If pLabel.emplacement = "" Then
            pLabel.emplacement = " "
        End If
        g.DrawString(pLabel.emplacement, f12, Brushes.Black, x + g.MeasureString(pLabel.refMag, fRef).Width + 30, y)
        y += g.MeasureString(pLabel.emplacement, f12).Height
        ' Code barre EAN13
        If pLabel.gencodeFourn = "" Then
            pLabel.gencodeFourn = " "
        End If
        g.DrawString(pLabel.gencodeFourn, f12, Brushes.Black, x, y)
        ' Ref Fourn
        If pLabel.refFourn = "" Then
            pLabel.refFourn = " "
        End If
        g.DrawString(pLabel.refFourn, f12, Brushes.Black, x + g.MeasureString(pLabel.refMag, fRef).Width + 30, y)
        y += g.MeasureString(pLabel.refFourn, f12).Height
        Dim img As Image
        If pLabel.gencodeMag <> "" Then
            Dim barCodeEAN As CodeEan13BarcodeDraw = New CodeEan13BarcodeDraw(CodeEan13Checksum.Instance)
            img = barCodeEAN.Draw(Left(pLabel.gencodeMag, 12), 25, 1)
        Else
            ' Gencode 39 Reference
            Dim barCode39 As Code39BarcodeDraw = BarcodeDrawFactory.Code39WithoutChecksum
            img = barCode39.Draw(pLabel.refMag, 25, 1)
        End If

        g.DrawImageUnscaled(img, x, y)
        y += img.Height
        ' Padding
        y += padding

        ' New column
        x += width - margin

        ' Break line
        If (x + pLabel.format.width) >= maxWidth Then
            x = 0
        Else
            y = y - pLabel.format.height
        End If

        ' Break Page
        If (y + pLabel.format.height) > maxHeight Then
            Return True
        End If

        Return False
    End Function

    Private Function DrawLabelF4(ByRef pLabel As PrintLabel, ByRef g As Graphics, ByRef maxWidth As Integer, ByRef maxHeight As Integer, ByRef x As Integer, ByRef y As Integer) As Boolean

        Dim pen As New Pen(Color.Gray, 1) With {
            .DashPattern = {2, 2}
        }
        Dim strFrm As New StringFormat() With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }
        ' maxHeight = 584
        Dim paddingHeight = 171
        y += paddingHeight
        Dim fRef As New Font("Arial", 80, FontStyle.Bold)
        Dim rect1 As New Rectangle(x, y, pLabel.format.width, fRef.Height)
        g.DrawString(pLabel.refMag, fRef, Brushes.Black, rect1, strFrm)

        y += rect1.Height

        Dim fDes As New Font("Arial", 25, FontStyle.Regular)
        Dim rect2 As New Rectangle(x, y, pLabel.format.width, fDes.Height * 2)
        g.DrawString(pLabel.designation, fDes, Brushes.Black, rect2, strFrm)

        y += rect2.Height

        Dim img As Image
        If pLabel.gencodeMag <> "" Then
            Dim barCodeEAN As CodeEan13BarcodeDraw = New CodeEan13BarcodeDraw(CodeEan13Checksum.Instance)
            img = barCodeEAN.Draw(Left(pLabel.gencodeMag, 12), 41, 2)
        Else
            ' Gencode 39 Reference
            Dim barCode39 As Code39BarcodeDraw = BarcodeDrawFactory.Code39WithoutChecksum
            img = barCode39.Draw(pLabel.refMag, 41, 2)
        End If
        g.DrawImageUnscaled(img, maxWidth / 2 - img.Width / 2, y)

        y += img.Height

        y += paddingHeight + 1

        g.DrawLine(pen, x, y, maxWidth, y)

        ' Break Page
        If (y + pLabel.format.height) > maxHeight Then
            Return True
        End If
        Return False
    End Function

    Private Function DrawLabelF5(ByRef pLabel As PrintLabel, ByRef g As Graphics, ByRef maxWidth As Integer, ByRef maxHeight As Integer, ByRef x As Integer, ByRef y As Integer) As Boolean
        Dim pen As New Pen(Color.Gray, 1) With {
    .DashPattern = {2, 2}
}
        Dim strFrm As New StringFormat() With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }
        ' maxHeight = 584
        Dim paddingHeight = 110
        y += paddingHeight
        Dim fRef As New Font("Arial", 80, FontStyle.Bold)
        Dim rect1 As New Rectangle(x, y, pLabel.format.width, fRef.Height)
        g.DrawString(pLabel.refMag, fRef, Brushes.Black, rect1, strFrm)

        y += rect1.Height

        Dim fDes As New Font("Arial", 25, FontStyle.Regular)
        Dim rect2 As New Rectangle(x, y, pLabel.format.width, fDes.Height * 2)
        g.DrawString(pLabel.designation, fDes, Brushes.Black, rect2, strFrm)

        y += rect2.Height

        Dim fPx As New Font("Arial", 79, FontStyle.Bold)
        Dim rect3 As New Rectangle(x, y, pLabel.format.width, fPx.Height)
        g.DrawString($"{pLabel.prix} € TTC", fPx, Brushes.Black, rect3, strFrm)
        y += rect3.Height

        Dim img As Image
        If pLabel.gencodeMag <> "" Then
            Dim barCodeEAN As CodeEan13BarcodeDraw = New CodeEan13BarcodeDraw(CodeEan13Checksum.Instance)
            img = barCodeEAN.Draw(Left(pLabel.gencodeMag, 12), 41, 2)
        Else
            ' Gencode 39 Reference
            Dim barCode39 As Code39BarcodeDraw = BarcodeDrawFactory.Code39WithoutChecksum
            img = barCode39.Draw(pLabel.refMag, 41, 2)
        End If
        g.DrawImageUnscaled(img, maxWidth / 2 - img.Width / 2, y)

        y += img.Height

        y += paddingHeight + 1

        g.DrawLine(pen, x, y, maxWidth, y)

        ' Break Page
        If (y + pLabel.format.height) > maxHeight Then
            Return True
        End If
        Return False
    End Function

    Public Overrides Function GetPreviewImage() As Image
        Dim f As New Font("Arial", 20, FontStyle.Regular)
        Dim sf As New StringFormat()
        sf.Alignment = StringAlignment.Center
        
        Dim imgWithText = New Bitmap(width, height)
        Dim bgRect As Rectangle = New Rectangle(0, 0, width, height)
        
        ' Create a graphics based on image & text
        With Graphics.FromImage(imgWithText)
            .FillRectangle(Brushes.White, bgRect)
            .DrawString("Pas de prévisu", f, Brushes.Black, width/2, (height/2) - (f.Height/2), sf)
        End With

        Return imgWithText
    End Function
End Class
