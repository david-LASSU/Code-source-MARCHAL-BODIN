Imports System.Data.SqlClient
Imports MBCore.Model

Public Class EtiquetteRepository
    Inherits BaseCialAbstract

    ''' <summary>
    ''' @DEPRECATED
    ''' </summary>
    ''' <param name="doPiece"></param>
    ''' <returns></returns>
    Public Function GetArticlesByPiece(ByVal doPiece As String) As DataTable
        Using cnx As New SqlConnection(cnxString)
            cnx.Open()
            Using cmd As SqlCommand = cnx.CreateCommand
                cmd.CommandText = "SELECT * FROM ETQARTICLEVIEW V
                                    JOIN F_DOCLIGNE DL ON DL.AR_Ref = V.AR_Ref AND DL.DO_Piece = @doPiece
                                    WHERE V.FORMAT = 'FORMAT_2'"
                cmd.Parameters.AddWithValue("@doPiece", doPiece)

                Using reader As SqlDataReader = cmd.ExecuteReader
                    Dim dt As New DataTable
                    dt.Load(reader)

                    Return dt
                End Using
            End Using
        End Using
    End Function

    Public Function GetArticles(ByVal filtre As ArticleFilter) As DataTable
        Using cnx As New SqlConnection(cnxString)
            cnx.Open()
            Using cmd As SqlCommand = cnx.CreateCommand
                cmd.CommandText = "SELECT DISTINCT V.* FROM ETQARTICLEVIEW V "

                'cmd.CommandText &= " WHERE 1 = 1 "
                ' Voir plus haut l'init du combobox
                Select Case filtre.SearchType
                    Case Is = 6
                        cmd.CommandText &= " JOIN F_DOCLIGNE DL ON DL.AR_Ref = V.AR_Ref WHERE DL.DO_Piece = @value "
                        cmd.Parameters.AddWithValue("@value", filtre.SearchValue)
                    Case Is = 0
                        ' On ne veux que les mises à jour
                        cmd.CommandText &= " WHERE V.ETQSTATUS = 1 AND V.AS_QteMini > 0 "
                    Case Is = 1
                        ' Recherche par emplacement
                        cmd.CommandText &= " WHERE  V.DP_Code LIKE @value + '%' "
                        cmd.Parameters.AddWithValue("@value", filtre.SearchValue)
                    Case Is = 2
                        ' Recherche par référence
                        cmd.CommandText &= " WHERE  V.AR_Ref LIKE @value + '%' "
                        cmd.Parameters.AddWithValue("@value", filtre.SearchValue)
                    Case Is = 3
                        ' Recherche par fournisseur
                        cmd.CommandText &= " WHERE V.CT_Num LIKE @value "
                        cmd.Parameters.AddWithValue("@value", filtre.SearchValue)
                    Case Is = 4
                        ' Recherche par code barre
                        cmd.CommandText &= " WHERE V.GencodFourn LIKE @value + '%' "
                        cmd.Parameters.AddWithValue("@value", filtre.SearchValue)
                    Case Is = 5
                        ' Recherche par ref fourn
                        cmd.CommandText &= " WHERE V.RefFourn LIKE @value + '%' "
                        cmd.Parameters.AddWithValue("@value", filtre.SearchValue)
                    Case Else
                        Throw New Exception("Index de choix incorrect")
                End Select

                If filtre.Sommeil = True Then
                    cmd.CommandText &= " AND V.AR_Sommeil = 0 "
                End If

                If filtre.NoEmpVide = True Then
                    cmd.CommandText &= " AND V.DP_Code <> '' "
                End If

                If filtre.SuiviStock = True Then
                    cmd.CommandText &= " AND V.AS_QteMini > 0 "
                End If

                If filtre.StockPositif = True Then
                    cmd.CommandText &= " AND V.AS_QteSto > 0 "
                End If

                cmd.CommandText &= "ORDER BY V.DP_Code ASC, V.AR_Ref ASC"
                Using reader As SqlDataReader = cmd.ExecuteReader
                    Dim dt As DataTable = New DataTable
                    dt.Load(reader)

                    Return dt
                End Using
            End Using
        End Using
    End Function

    Public Function GetEmplacements(filter As String) As DataTable
        Using cnx As New SqlConnection(cnxString)
            cnx.Open()

            Using cmd As SqlCommand = cnx.CreateCommand
                cmd.CommandText = "SELECT DP_Code
                    FROM F_DEPOTEMPL DP
                    JOIN F_DEPOT DE ON DE.DE_No = DP.DE_No
                    WHERE DE_Intitule = @dbName "
                If filter <> "" Then
                    cmd.CommandText &= " AND DP_Code LIKE @dpCode + '%' "
                    cmd.Parameters.AddWithValue("@dpCode", filter)
                End If
                cmd.CommandText &= " ORDER BY DP_Code"
                cmd.Parameters.AddWithValue("@dbName", dbName.Replace("DEV", ""))

                Using reader As SqlDataReader = cmd.ExecuteReader
                    Dim dt As New DataTable
                    dt.Load(reader)

                    Return dt
                End Using
            End Using
        End Using
    End Function

    Public Sub resetEtiquette(ByVal refMag As String)
        Using cnx As New SqlConnection(cnxString)
            cnx.Open()
            Using cmd As SqlCommand = cnx.CreateCommand
                With cmd
                    .CommandType = CommandType.StoredProcedure
                    .CommandText = "MAJ_ETIQUETTE"
                    .Parameters.AddWithValue("@ref", refMag)
                    .ExecuteNonQuery()
                End With
            End Using
        End Using
    End Sub
End Class
