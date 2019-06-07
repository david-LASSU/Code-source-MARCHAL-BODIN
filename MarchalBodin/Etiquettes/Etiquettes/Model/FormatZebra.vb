Public Class FormatZebra
    Inherits Format
    Implements IFormatZebra

    Public Overrides ReadOnly Property type As String
        Get
            Return "zebra"
        End Get
    End Property

    ''' <summary>
    ''' Chaine ZPL à imprimer
    ''' </summary>
    ''' <remarks></remarks>
    Private _zplString As String
    Public Property zplString As String Implements IFormatZebra.zplString
        Get
            Return _zplString
        End Get
        Set(value As String)
            _zplString = value
        End Set
    End Property

    Private _B3String As String
    Public Property B3String As String Implements IFormatZebra.B3String
        Get
            Return _B3String
        End Get
        Set(value As String)
            _B3String = value
        End Set
    End Property
    Private _BEString As String
    Public Property BEString As String Implements IFormatZebra.BEString
        Get
            Return _BEString
        End Get
        Set(value As String)
            _BEString = value
        End Set
    End Property

    ''' <summary>
    ''' Url de l'image de preview
    ''' </summary>
    ''' <remarks></remarks>
    Private _url As String
    Public Property url As String Implements IFormatZebra.url
        Get
            Return _url
        End Get
        Set(value As String)
            _url = value
        End Set
    End Property

    Public Function GetZplStringFromLabel(ByVal pLabel As PrintLabel) As String
        Dim gencodeMag As String

        ' Si un code barre existe on l'imprime avec la ligne zpl correspondante
        If pLabel.gencodeMag <> "" Then
            gencodeMag = String.Format(BEString, pLabel.gencodeMag)
        Else
            gencodeMag = String.Format(B3String, pLabel.refMag)
        End If

        Return String.Format(
            zplString,
            pLabel.designation,
            pLabel.refMag,
            pLabel.colisage,
            pLabel.emplacement,
            pLabel.refFourn,
            pLabel.gencodeFourn,
            pLabel.prix,
            gencodeMag,
            pLabel.conditionnement,
            pLabel.emplSec,
            pLabel.dateImpr)
    End Function

    Public Overrides Function GetPreviewImage() As Image
        Dim zUrl As String = url + String.Format(
            zplString,
            "Désignation",
            "0000-0000/XXX",
            10,
            "B20 E20 R20 N20",
            "refFourn",
            "gencodeFourn",
            "1000,00",
            String.Format(BEString, "3253561332388"),
            "Rouleau",
            "B28 E28 R28 N28",
            Date.Now.ToShortDateString)
        Dim req As Net.HttpWebRequest = DirectCast(Net.WebRequest.Create(zUrl), Net.HttpWebRequest)
        Dim res As Net.HttpWebResponse = DirectCast(req.GetResponse, Net.HttpWebResponse)
        Dim img As Image = New Bitmap(res.GetResponseStream)
        res.Close()

        Return img
    End Function
End Class
