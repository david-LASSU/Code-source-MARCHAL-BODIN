Public Class ColorLegend
    Private _color As Color
    Private _definition As String

    Public Property color As Color
        Get
            Return _color
        End Get
        Set(ByVal value As Color)
            _color = value
        End Set
    End Property

    Public Property definition As String
        Get
            Return _definition
        End Get
        Set(value As String)
            _definition = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal color As Color, ByVal definition As String)
        _color = color
        _definition = definition
    End Sub
End Class
