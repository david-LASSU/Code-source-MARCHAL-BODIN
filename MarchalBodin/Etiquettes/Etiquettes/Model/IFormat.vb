Public Interface IFormat
    ''' <summary>
    ''' Index du format eg. FORMAT_1
    ''' </summary>
    ''' <remarks></remarks>
    Property key As String

    ''' <summary>
    ''' Description du format
    ''' </summary>
    ''' <remarks></remarks>
    Property description As String

    ''' <summary>
    ''' Largeur de l'image en pixel
    ''' </summary>
    Property width As Integer

    ''' <summary>
    ''' Hauteur de l'image en pixel
    ''' </summary>
    Property height As Integer

    ''' <summary>
    ''' Défini le type d'imprimante (zebra ou normal)
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property type As String

    ''' <summary>
    ''' Retourne l'image de previsu du format
    ''' </summary>
    ''' <returns></returns>
    Function GetPreviewImage As Image
End Interface
