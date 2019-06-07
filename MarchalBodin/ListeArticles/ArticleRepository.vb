Imports System.Data.SqlClient
Imports Zen.Barcode

Public Interface IDataPageRetriever

    Function SupplyPageOfData(
        ByVal lowerPageBoundary As Integer, ByVal rowsPerPage As Integer) _
        As DataTable

End Interface

Public Class ArticleRepository
    Implements IDataPageRetriever

    'Private command As SqlCommand
    Private tableName As String = "F_ARTICLE"
    'Private connection As SqlConnection

    Private cnxString As String

    Private selectParams As New Dictionary(Of String, String) From {
        {"AR_Ref", "A.AR_Ref"},
        {"Sommeil", "CASE WHEN A.AR_Sommeil = 0 THEN 'Non' ELSE 'Oui' END"},
        {"SUPPRIME", "A.SUPPRIME"},
        {"SUPPRIME_USINE", "A.SUPPRIME_USINE"},
        {"AR_Design", "A.AR_Design"},
        {"AR_CodeBarre", "A.AR_CodeBarre"},
        {"AF_RefFourniss", "AF.AF_RefFourniss"},
        {"FA_CodeFamille", "A.FA_CodeFamille"},
        {"CT_Num", "CT.CT_Num"},
        {"CT_Intitule", "CT.CT_Intitule"},
        {"U_Intitule", "PU.U_Intitule"},
        {"AR_PrixVen", "A.AR_PrixVen"},
        {"AC_PrixVen", "AC1.AC_PrixVen"},
        {"DP_Code", "D.DP_Code"},
        {"SuiviEnStock", "CASE WHEN A.AR_SuiviStock <> 0 AND ISNULL(S.AS_QteMini, 0) < 1 THEN 'Non' ELSE 'Oui' END"},
        {"QteSto", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QteSto ELSE NULL END"},
        {"QteCom", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QteCom ELSE NULL END"},
        {"QtePrepa", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QtePrepa ELSE NULL END"},
        {"QteRes", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QteRes ELSE NULL END"},
        {"StockATerme", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QteSto - S.AS_QtePrepa - S.AS_QteRes - S.AS_QteResCM + S.AS_QteCom + S.AS_QteComCM ELSE NULL END"},
        {"QteMini", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QteMini ELSE NULL END"},
        {"QteMaxi", "CASE WHEN A.AR_SuiviStock <> 0 THEN S.AS_QteMaxi ELSE NULL END"},
        {"refInterne", "(SELECT CONCAT('A', CAST(A.AR_PrixAch * 100 AS decimal(24,0)), 'C', CAST(A.AR_Coef * 100 AS DECIMAL(24,0))))"}
    }

    Public Sub New(ByVal connectionString As String)
        'connection = New SqlConnection(connectionString)
        'connection.Open()
        cnxString = connectionString
    End Sub

    Private rowCountValue As Integer = -1

    Public ReadOnly Property RowCount() As Integer
        Get
            ' Return the existing value if it has already been determined.
            If Not rowCountValue = -1 Then
                Return rowCountValue
            End If

            ' Retrieve the row count from the database.
            Using cnx As New SqlConnection(cnxString)
                cnx.Open

                using cmd As SqlCommand = New SqlCommand("SET ARITHABORT ON", cnx)
                    cmd.ExecuteNonQuery

                    cmd.CommandText = "SET NOCOUNT ON; SELECT COUNT(*) "
                    prepareSqlCommandBase(cmd)
                    cmd.CommandText &= " option(recompile)"
                    rowCountValue = CInt(cmd.ExecuteScalar())
                End Using
            End Using

            Return rowCountValue
        End Get
    End Property

    Private columnsValue As DataColumnCollection

    Public ReadOnly Property Columns() As DataColumnCollection
        Get
            ' Return the existing value if it has already been determined.
            If columnsValue IsNot Nothing Then
                Return columnsValue
            End If

            ' Retrieve the column information from the database.
            Using cnx As New SqlConnection(cnxString)
                cnx.Open

                Using cmd As SqlCommand = New SqlCommand("SET ARITHABORT ON", cnx)
                    cmd.ExecuteNonQuery

                    cmd.CommandText = SqlSelect()
                    prepareSqlCommandBase(cmd)
                    cmd.CommandText &= " option(recompile)"

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        Dim table As New DataTable()
                        table.Locale = System.Globalization.CultureInfo.InvariantCulture
                        table.Load(reader)
                        columnsValue = table.Columns
                    End Using

                    'Dim adapter As New SqlDataAdapter()
                    'adapter.SelectCommand = cmd
                    'Dim table As New DataTable()
                    'table.Locale = System.Globalization.CultureInfo.InvariantCulture
                    'adapter.FillSchema(table, SchemaType.Source)
                    
                End Using
            End Using
            
            Return columnsValue
        End Get
    End Property

    'Private adapter As New SqlDataAdapter()

    Public Function SupplyPageOfData(
        ByVal lowerPageBoundary As Integer, ByVal rowsPerPage As Integer) _
        As DataTable Implements IDataPageRetriever.SupplyPageOfData

        ' Retrieve the specified number of rows from the database, starting
        ' with the row specified by the lowerPageBoundary parameter.
        Using cnx As New SqlConnection(cnxString)
            cnx.Open

            Using cmd As SqlCommand = New SqlCommand("SET ARITHABORT ON", cnx)
                cmd.ExecuteNonQuery

                cmd.CommandText = SqlSelect()
                prepareSqlCommandBase(cmd)

                Dim sw As String = If(Form1.SortWay = Windows.Forms.SortOrder.Descending, "DESC", "ASC")
                Dim sc As String = selectParams.Item(Form1.SortCol)
                If Not sc.Contains(".") Then
                    sc = Form1.SortCol
                End If
                cmd.CommandText &= "ORDER BY " & sc & " " & sw & " OFFSET " & lowerPageBoundary & " ROWS FETCH NEXT " & rowsPerPage & " ROWS ONLY "
                cmd.CommandText &= "option(recompile)"
                Debug.Print(cmd.CommandText)

                Using reader As SqlDataReader = cmd.ExecuteReader
                    Dim table As New DataTable()
                    table.Locale = System.Globalization.CultureInfo.InvariantCulture
                    table.Load(reader)
                    Return table
                End Using

                'Using adapter As New SqlDataAdapter
                '    adapter.SelectCommand = cmd
                '    Dim table As New DataTable()
                '    table.Locale = System.Globalization.CultureInfo.InvariantCulture
                '    adapter.Fill(table)

                '    Return table
                'End Using
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Concat columns to a Select String
    ''' </summary>
    ''' <returns></returns>
    Private Function SqlSelect() As String
        Dim sql As String = "SET NOCOUNT ON; SELECT "
        For Each pair As KeyValuePair(Of String, String) In selectParams
            sql &= pair.Value & " AS " & pair.Key
            If pair.Key <> selectParams.Last.Key Then
                sql &= ", "
            End If
        Next
        Return sql
    End Function

    Private Sub prepareSqlCommandBase(ByRef cmd As SqlCommand)

        cmd.CommandText += " FROM F_ARTICLE A
            LEFT JOIN P_UNITE PU ON PU.cbMarq = A.AR_UniteVen
            LEFT JOIN F_ARTFOURNISS AF ON AF.AR_Ref = A.AR_Ref AND AF.AF_Principal = 1
            LEFT JOIN F_COMPTET CT ON AF.CT_Num = CT.CT_Num
            LEFT JOIN F_ARTSTOCK S ON S.AR_Ref = A.AR_Ref
            LEFT JOIN F_DEPOTEMPL D ON S.DE_No = D.DE_No AND S.DP_NoPrincipal = D.DP_No
            LEFT JOIN F_ARTCLIENT AC1 ON AC1.AR_Ref = A.AR_Ref AND AC1.AC_Categorie = 1"

        Dim joinPart As New List(Of String)
        Dim wherePart As New List(Of String)

        ' Désignation contient
        ' Exemple :
        'SELECT AR_Ref, AR_Design, REFFOURNS
        'FROM F_ARTICLE
        'WHERE CONTAINS(*, '"mesure*" AND "fat*"')
        Dim design As String = Form1.DesignTextBox.Text.Trim
        If design <> "" Then

            ' Recherche designation, ref fourn etc ....
            Dim desPart As New List(Of String)
            Dim fullTextQuery As String
            With design
                If .Contains(" ") Then
                    For Each part As String In .Split(" ")
                        desPart.Add(String.Format("""{0}*""", part))
                    Next
                    fullTextQuery = String.Join(" AND ", desPart.ToArray)
                Else
                    fullTextQuery = String.Format("""{0}*""", .ToString)
                End If
            End With

            wherePart.Add("(CONTAINS(A.*, @fullTextQuery) OR A.REFFOURNS LIKE '%' + @design + '%')")
            cmd.Parameters.AddWithValue("@fullTextQuery", fullTextQuery)
            cmd.Parameters.AddWithValue("@design", design)
        End If

        If Form1.CodeBarreTextBox.Text.Trim <> ""
                Using cnx As New SqlConnection(cnxString)
                cnx.Open

                Using cmd2 As SqlCommand = New SqlCommand("SET ARITHABORT ON", cnx)
                    cmd2.ExecuteNonQuery

                    cmd2.CommandText = "SET NOCOUNT ON; SELECT TOP 1 A.AR_Ref
                        FROM F_ARTICLE A
                        LEFT JOIN F_ARTFOURNISS AF ON AF.AR_Ref = A.AR_Ref
                        LEFT JOIN F_CONDITION CO ON CO.AR_Ref = A.AR_Ref
                        LEFT JOIN F_TARIFGAM TG ON TG.AR_Ref = A.AR_Ref
                        WHERE (A.AR_CodeBarre = @barcode OR CO.CO_CodeBarre = @barcode OR TG.TG_CodeBarre = @barcode OR AF.AF_CodeBarre = @barcode)"
                    cmd2.Parameters.AddWithValue("@barcode", Form1.CodeBarreTextBox.Text.Trim)

                    Using r2 As SqlDataReader = cmd2.ExecuteReader
                        If r2.HasRows
                            r2.Read
                            ' c'est un code barre on requete direct sur la ref
                            wherePart.Add("A.AR_Ref = @design")
                            cmd.Parameters.AddWithValue("@design", r2.Item("AR_Ref").ToString)
                        End If
                    End Using
                End Using
            End Using
        End If
        If Form1.RefComboBox.SelectedIndex > 0 Then
            Select Case Form1.RefComboBox.SelectedIndex
                Case 1
                    ' Contient
                    wherePart.Add("A.AR_Ref LIKE '%' + @arRef + '%'")
                Case 2
                    ' Est compris entre
                    wherePart.Add("A.AR_Ref BETWEEN @arRef AND @arRef2")
                    cmd.Parameters.AddWithValue("@arRef2", Form1.RefTextBox2.Text)
                Case 3
                    ' Commence par
                    wherePart.Add("A.AR_Ref LIKE @arRef + '%'")
                Case 4
                    ' Se termine par
                    wherePart.Add("A.AR_Ref LIKE '%' + @arRef")
                Case 5
                    ' =
                    wherePart.Add("A.AR_Ref LIKE @arRef")
                Case 6
                    ' <>
                    wherePart.Add("A.AR_Ref <> @arRef")
                Case 7
                    ' >=
                    wherePart.Add("A.AR_Ref >= @arRef")
                Case 8
                    ' >
                    wherePart.Add("A.AR_Ref > @arRef")
                Case 9
                    ' <=
                    wherePart.Add("A.AR_Ref <= @arRef")
                Case 10
                    ' <
                    wherePart.Add("A.AR_Ref < @arRef")
                Case Else
                    ' Do nothing
            End Select
            cmd.Parameters.AddWithValue("@arRef", Form1.RefTextBox1.Text)
        End If

        Select Case Form1.ArtStatusComboBox.SelectedIndex
            Case 0
                wherePart.Add("A.AR_Sommeil = 0")
            Case 1
                wherePart.Add("A.AR_Sommeil = 1")
            Case Else
                ' Do nothing
        End Select

        ' Supprimé
        If Form1.SuppCheckBox.Checked = True Then
            wherePart.Add("SUPPRIME LIKE 'Oui'")
        End If

        ' Supprimé usine
        If Form1.SuppUsineCheckBox.Checked = True Then
            wherePart.Add("SUPPRIME_USINE LIKE 'Oui'")
        End If

        ' Recherche par fournisseur
        Dim ctNum As String = Form1.GetCtNum()
        If ctNum <> String.Empty Then
            joinPart.Add("JOIN F_ARTFOURNISS AF2 ON AF2.AR_Ref = A.AR_Ref AND AF2.CT_Num = @ctNum")
            cmd.CommandText = cmd.CommandText.Replace("AF.AF_RefFourniss", "AF2.AF_RefFourniss")
            cmd.Parameters.AddWithValue("@ctNum", ctNum)

            ' Fournisseur principal uniquement
            If Form1.FournPrincComboBox.SelectedIndex <> 2 Then
                wherePart.Add("AF2.AF_Principal = @fournPrinc")
                cmd.Parameters.AddWithValue("@fournPrinc", If(Form1.FournPrincComboBox.SelectedIndex = 0, 1, 0))
            End If
        End If

        ' Catégorie
        If Form1.TreeView1.SelectedNode.Name <> "__ALL__" Then
            With Form1.TreeView1.SelectedNode
                wherePart.Add("A.CL_No" & (.Level + 1) & " = @categorie")
                cmd.Parameters.AddWithValue("@categorie", .Name)
            End With
        End If

        ' Suivi en Stock
        If Form1.SuiviCheckBox.Checked = True Then
            wherePart.Add("ISNULL(S.AS_QteMini, 0) > 0")
        End If

        ' Emplacement
        If Form1.EmplacementTextBox.Text <> "" Then
            wherePart.Add("D.DP_Code LIKE @dpCode + '%'")
            cmd.Parameters.AddWithValue("@dpCode", Form1.EmplacementTextBox.Text)
        End If

        ' Concaténation des clauses
        For Each j As String In joinPart
            cmd.CommandText += j + " "
        Next

        Dim clause As String = " WHERE"
        For Each w As String In wherePart
            cmd.CommandText += String.Format(" {0} {1} ", clause, w)
            clause = "AND"
        Next
    End Sub

    Public Function getCatTarif(ByVal arRef As String) As DataTable
        Using cnx As New SqlConnection(cnxString)
            cnx.Open

            Using cmd As New SqlCommand("SET ARITHABORT ON",cnx)
                cmd.Parameters.AddWithValue("@arRef", arRef)

                ' Cherche d'abord conditionnement
                cmd.CommandText = "SELECT ISNULL(CO.CO_Ref, CO.AR_Ref) AS AR_Ref, CO.EC_Enumere AS Conditionnement, P.CT_Intitule, AC.AC_Categorie, ISNULL(AC.AC_Remise, 0) AS AC_Remise, 
                    TC.TC_Prix - TC.TC_Prix*ISNULL(AC.AC_Remise,0)/100 AS pxCat
                    FROM F_CONDITION CO
                    JOIN F_TARIFCOND TC ON CO.CO_No = TC.CO_No
                    JOIN F_ARTCLIENT AC ON AC.AR_Ref = TC.AR_Ref AND AC.AC_Categorie = CAST(REPLACE(TC.TC_RefCF, 'a','') AS INT)
                    JOIN P_CATTARIF P ON P.cbMarq = CAST(REPLACE(TC.TC_RefCF, 'a','') AS INT)
                    WHERE TC.AR_Ref = @arRef"

               Using reader As SqlDataReader = cmd.ExecuteReader
                    If reader.HasRows
                        Dim table As New DataTable
                        table.Load(reader)

                        Return table
                    End If
               End Using

                ' Sinon article normal
                cmd.CommandText = "SELECT A.AR_Ref, '' AS Conditionnement, P.CT_Intitule, AC.AC_Categorie, AC.AC_Remise,
                        CASE 
                            WHEN AC.AC_PrixVen > 0 THEN AC.AC_PrixVen 
                            WHEN AC.AC_Categorie = 7 THEN ISNULL(A.AR_PrixAch,0) * 1.1
                            WHEN AC.AC_Categorie = 14 THEN ISNULL(A.AR_PrixAch,0) * 1
                            ELSE A.AR_PrixVen - (A.AR_PrixVen * AC_Remise/100) 
                        END AS pxCat
                    FROM F_ARTICLE A
                    JOIN F_ARTCLIENT AC ON AC.AR_Ref = A.AR_Ref
                    JOIN P_CATTARIF P ON P.cbMarq = AC.AC_Categorie
                    WHERE A.AR_Ref = @arRef
                    ORDER BY AC.AC_Categorie"
                
                Using reader As SqlDataReader = cmd.ExecuteReader
                    Dim table As New DataTable
                    table.Load(reader)

                    Return table
                End Using
            End Using
        End Using
    End Function
End Class
