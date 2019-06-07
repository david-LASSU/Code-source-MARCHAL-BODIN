Imports Etiquettes

Public MustInherit Class Format
    Implements IFormat
    ''' <summary>
    ''' Index du format eg. FORMAT_1
    ''' </summary>
    ''' <remarks></remarks>
    Private _key As String
    Public Property key As String Implements IFormat.key
        Get
            Return _key
        End Get
        Set(value As String)
            _key = value
        End Set
    End Property

    ''' <summary>
    ''' Description du format
    ''' </summary>
    ''' <remarks></remarks>
    Private _description As String
    Public Property description As String Implements IFormat.description
        Get
            Return _description
        End Get
        Set(value As String)
            _description = value
        End Set
    End Property

    ''' <summary>
    ''' Largeur de l'image en pixel
    ''' </summary>
    Private _width As Integer
    Public Property width As Integer Implements IFormat.width
        Get
            Return _width
        End Get
        Set(value As Integer)
            _width = value
        End Set
    End Property

    ''' <summary>
    ''' Hauteur de l'image en pixel
    ''' </summary>
    Private _height As Integer
    Public Property height As Integer Implements IFormat.height
        Get
            Return _height
        End Get
        Set(value As Integer)
            _height = value
        End Set
    End Property

    ''' <summary>
    ''' Défini le type d'imprimante
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride ReadOnly Property type As String Implements IFormat.type

    Public MustOverride Function GetPreviewImage() As Image Implements IFormat.GetPreviewImage

End Class
