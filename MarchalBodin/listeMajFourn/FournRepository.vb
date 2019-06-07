Imports System.Data.SqlClient
Imports MBCore.Model
Public Class FournRepository
    Inherits BaseCialAbstract
    Public Function GetFourns(ctIntitule As String, actif As Boolean, sommeil As Boolean) As DataTable
        Using cnx As New SqlConnection(cnxString)
            cnx.Open()
            Using cmd As SqlCommand = cnx.CreateCommand
                cmd.CommandText = "SELECT DISTINCT F.CT_Num, CT_Intitule, DATE_MAJ_TARIF, Sommeil = CASE WHEN CT_Sommeil = 0 THEN 'Oui' ELSE 'Non' END, MAJ_URGENTE, 
                        ISNULL(EN_COURS_DE_MAJ, '') AS EN_COURS_DE_MAJ, ISNULL(INFO_MAJ_TARIF,'') AS INFO_MAJ_TARIF,
                        (SELECT COUNT(*) FROM F_ARTFOURNISS AF WHERE AF.CT_Num = F.CT_Num AND ISNULL(AF_CodeBarre, '') = '') AS NB_CBVIDES,
                        (SELECT COUNT(*) FROM F_ARTFOURNISS AF WHERE AF.CT_Num = F.CT_Num) AS NB_ART
                        FROM F_COMPTET F
                        JOIN F_ARTFOURNISS AF ON F.CT_Num = AF.CT_Num"

                If ctIntitule <> "" Then
                    cmd.CommandText &= " AND (CT_Intitule LIKE '%' + @value + '%' OR F.CT_Num LIKE '%' + @value + '%')"
                    cmd.Parameters.AddWithValue("@value", ctIntitule)
                End If

                If actif = True And sommeil = False Then
                    cmd.CommandText &= " AND CT_Sommeil = 0"
                End If

                If sommeil = True And actif = False Then
                    cmd.CommandText &= " AND CT_Sommeil = 1"
                End If

                cmd.CommandText &= " ORDER BY DATE_MAJ_TARIF DESC"

                Using reader As SqlDataReader = cmd.ExecuteReader
                    Dim dt As New DataTable
                    dt.Load(reader)
                    Return dt
                End Using
            End Using
        End Using
    End Function
End Class
