Public Interface IFormatZebra
    Inherits IFormat
    ''' <summary>
    ''' Chaine ZPL à imprimer
    ''' </summary>
    ''' <remarks></remarks>
    Property zplString As String

    ''' <summary>
    ''' chaine du code 39
    ''' </summary>
    ''' <returns></returns>
    Property B3String As String

    ''' <summary>
    ''' chaine du EAN 13
    ''' </summary>
    ''' <returns></returns>
    Property BEString As String

    ''' <summary>
    ''' Url de l'image de preview
    ''' </summary>
    ''' <remarks></remarks>
    Property url As String
End Interface
