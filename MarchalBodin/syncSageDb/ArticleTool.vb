Imports System.Data.SqlClient
Public Class ArticleTool
    Inherits Tool

    ' List of ARTCHKSYNCVIEW.COLUMNS
    Private vCols As New List(Of String) From {"AR_Ref", "csArticle", "csArtClientAgg", "csArtFournissAgg", "csArtGammeAgg", "csArtEnumRefAgg", "csArtTarifQteAgg", "csArtTarifGamAgg", "csNomencAgg", "csArtComptaAgg", "csArtCondAgg", "csTarifCondAgg", "CS"}

    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    ' TODO Temporaire, permet de mettre à jour les codes barres depuis les bases sociétés
    Public Sub MajCodesBarres()
        Try
            Log("[MAJ CODES BARRES]")

            Using cnxSource As New SqlConnection(dbSource.ToString)
                Using cmdSource As SqlCommand = cnxSource.CreateCommand
                    cmdSource.CommandText = String.Format(
                        "SELECT A.AR_Ref, AR_CodeBarre, AF.CT_Num 
                        FROM [F_ARTICLE] A 
                        JOIN [F_ARTFOURNISS] AF ON AF.AR_Ref = A.AR_Ref And AF.AF_CodeBarre = AR_CodeBarre 
                        WHERE CodeBarreState = 1",
                        cnxSource.DataSource,
                        cnxSource.Database
                    )
                    cnxSource.Open()

                    Using reader As SqlDataReader = cmdSource.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                Using cnxTarget As New SqlConnection(dbTarget.ToString)
                                    cnxTarget.Open()

                                    Using transaction As SqlTransaction = cnxTarget.BeginTransaction
                                        Try
                                            Log(String.Format("{0}:: {1}", reader.Item("AR_Ref"), reader.Item("AR_CodeBarre")))
                                            With cnxTarget.CreateCommand
                                                .Transaction = transaction
                                                .CommandText = String.Format("UPDATE {0}.[dbo].[F_ARTICLE] SET AR_CodeBarre = @codeBarre WHERE AR_Ref = @ref", cnxTarget.Database)
                                                .Parameters.AddWithValue("@codeBarre", reader.Item("AR_CodeBarre"))
                                                .Parameters.AddWithValue("@ref", reader.Item("AR_Ref"))
                                                .CommandTimeout = cmdTimeOut
                                                .ExecuteNonQuery()
                                                .Dispose()
                                            End With

                                            With cnxTarget.CreateCommand
                                                .Transaction = transaction
                                                .CommandText = String.Format(
                                                    "UPDATE {0}.[dbo].[F_ARTFOURNISS] SET AF_CodeBarre = @codeBarre WHERE AR_Ref = @ref AND CT_Num = @ctNum",
                                                    cnxTarget.Database
                                                )
                                                .Parameters.AddWithValue("@codeBarre", reader.Item("AR_CodeBarre"))
                                                .Parameters.AddWithValue("@ref", reader.Item("AR_Ref"))
                                                .Parameters.AddWithValue("@ctNum", reader.Item("CT_Num"))
                                                .CommandTimeout = cmdTimeOut
                                                .ExecuteNonQuery()
                                                .Dispose()
                                            End With

                                            transaction.Commit()

                                            ' Met à jour le status
                                            With cnxSource.CreateCommand
                                                .CommandText = "UPDATE [F_ARTICLE] SET CodeBarreState = 0 WHERE AR_Ref = @ref"
                                                .Parameters.AddWithValue("@ref", reader.Item("AR_Ref"))
                                                .CommandTimeout = cmdTimeOut
                                                .ExecuteNonQuery()
                                                .Dispose()
                                            End With

                                        Catch ex As Exception
                                            Log(String.Format("Commit Exception: {0}", ex.ToString))
                                            Try
                                                transaction.Rollback()
                                            Catch ex2 As Exception
                                                Log(String.Format("Rollback Exception Type: {0}", ex2.ToString))
                                            End Try
                                        End Try
                                    End Using
                                End Using
                            End While
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    ' Synchronise la table P_GAMME + Enumérés de gamme
    Public Sub MajGammes()
        Try
            Log("[MAJ GAMMES]")

            '
            ' P_GAMME
            '
            Using cnx As New SqlConnection(dbTarget.ToString)
                cnx.Open()
                Using cmd As SqlCommand = cnx.CreateCommand()
                    cmd.CommandText = String.Format(
                        "UPDATE L SET L.G_Intitule = R.G_Intitule, L.G_Type = R.G_Type
                        FROM {0}.[P_GAMME] L 
                        JOIN {1}.[P_GAMME] R ON R.cbMarq = L.cbMarq
                        WHERE L.G_Intitule <> R.G_Intitule",
                        dbTarget.sqlDbName,
                        dbSource.sqlDbName
                    )
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            '
            ' F_ENUMGAMME
            '
            ' Delete invalids gammes
            Using cnx As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnx.CreateCommand
                    cnx.Open()
                    cmd.CommandText = String.Format(
                        "DELETE R FROM {0}.[F_ENUMGAMME] R WHERE R.csEnumGamme NOT IN(SELECT csEnumGamme FROM {1}.[F_ENUMGAMME])",
                        dbTarget.sqlDbName,
                        dbSource.sqlDbName
                    )
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            ' Crée les ENUMGAMME
            Using cnx As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnx.CreateCommand
                    cnx.Open()
                    cmd.CommandText = String.Format(
                        "SELECT L.EG_Champ, L.EG_Ligne, L.EG_Enumere, L.EG_BorneSup FROM {0}.[F_ENUMGAMME] L 
                        LEFT JOIN {1}.[F_ENUMGAMME] R ON R.EG_Champ = L.EG_Champ AND R.EG_Enumere = L.EG_Enumere 
                        WHERE ISNULL(R.csEnumGamme, 0) = 0",
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                Using cnxTarget As New SqlConnection(dbTarget.ToString)
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        cnxTarget.Open()
                                        Dim fields As String = ""
                                        Dim values As String = ""

                                        For i As Integer = 0 To reader.FieldCount - 1
                                            If i > 0 Then
                                                fields &= ", "
                                                values &= ", "
                                            End If
                                            fields &= String.Format("{0}", reader.GetName(i))
                                            values &= String.Format("@{0}", reader.GetName(i))
                                            cmdTarget.Parameters.AddWithValue(reader.GetName(i), reader.Item(i))
                                        Next
                                        cmdTarget.CommandText = String.Format("INSERT INTO {0}.[dbo].[F_ENUMGAMME] ({1}) VALUES ({2})", cnxTarget.Database, fields, values)
                                        cmdTarget.CommandTimeout = cmdTimeOut
                                        cmdTarget.ExecuteNonQuery()
                                    End Using
                                End Using
                            End While
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub
    ' Synchronise les Paramètres de conditionnement et les énumérés de conditionnement
    Public Sub MajConditionnements()
        Try
            Log("[MAJ COND]")

            '
            ' P_CONDITIONNEMENT
            '
            Using cnx As New SqlConnection(dbTarget.ToString)
                cnx.Open()
                Using cmd As SqlCommand = cnx.CreateCommand()
                    cmd.CommandText = String.Format(
                        "UPDATE L SET L.P_Conditionnement = R.P_Conditionnement 
                        FROM {0}.[P_CONDITIONNEMENT] L 
                        JOIN {1}.[P_CONDITIONNEMENT] R ON R.cbMarq = L.cbMarq
                        WHERE L.P_Conditionnement <> R.P_Conditionnement",
                        dbTarget.sqlDbName,
                        dbSource.sqlDbName
                    )
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            '
            ' F_ENUMCOND
            '
            ' Delete invalids conds
            Using cnx As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnx.CreateCommand
                    cnx.Open()
                    cmd.CommandText = String.Format(
                        "DELETE R FROM {0}.[F_ENUMCOND] R WHERE R.csEnumCond NOT IN(SELECT csEnumCond FROM {1}.[F_ENUMCOND])",
                        dbTarget.sqlDbName,
                        dbSource.sqlDbName
                    )
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            ' Crée les ENUMCOND
            Using cnx As New SqlConnection(dbTarget.ToString)
                Using cmd As SqlCommand = cnx.CreateCommand
                    cnx.Open()
                    cmd.CommandText = String.Format(
                        "SELECT L.EC_Champ, L.EC_Enumere, L.EC_Quantite FROM {0}.[F_ENUMCOND] L 
                        LEFT JOIN {1}.[F_ENUMCOND] R ON R.EC_Champ = L.EC_Champ AND R.EC_Enumere = L.EC_Enumere 
                        WHERE ISNULL(R.csEnumCond, 0) = 0",
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )

                    Using reader As SqlDataReader = cmd.ExecuteReader
                        If reader.HasRows Then
                            While reader.Read
                                Using cnxTarget As New SqlConnection(dbTarget.ToString)
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        cnxTarget.Open()
                                        Dim fields As String = ""
                                        Dim values As String = ""

                                        For i As Integer = 0 To reader.FieldCount - 1
                                            If i > 0 Then
                                                fields &= ", "
                                                values &= ", "
                                            End If
                                            fields &= String.Format("{0}", reader.GetName(i))
                                            values &= String.Format("@{0}", reader.GetName(i))
                                            cmdTarget.Parameters.AddWithValue(reader.GetName(i), reader.Item(i))
                                        Next
                                        cmdTarget.CommandText = String.Format("INSERT INTO {0}.[dbo].[F_ENUMCOND] ({1}) VALUES ({2})", cnxTarget.Database, fields, values)
                                        cmdTarget.CommandTimeout = cmdTimeOut
                                        cmdTarget.ExecuteNonQuery()
                                    End Using
                                End Using
                            End While
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    Public Sub MajArticles()
        Try
            Log("[MAJ ARTICLES]")

            Using cnxSource As New SqlConnection(dbTarget.ToString)

                Using cmd As SqlCommand = cnxSource.CreateCommand
                    cnxSource.Open()
                    ' Requete très longue < 2 minutes
                    ' Ne peut pas récupérer toutes les colonnes car prend trop de place
                    ' on utilise la sous-requete juste après
                    cmd.CommandText = String.Format(
                        "SELECT L.AR_Ref AS L_AR_Ref, R.AR_Ref As R_AR_Ref
                        FROM {0}.[ARTCHKSYNCVIEW] L 
                        LEFT JOIN {1}.[ARTCHKSYNCVIEW] R ON R.AR_Ref = L.AR_Ref 
                        WHERE L.CS <> R.CS OR R.AR_Ref IS NULL
                        ORDER BY CASE WHEN L.csNomencAgg IS NULL THEN 1 ELSE 2 END, L.AR_Ref",
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )
                    cmd.CommandTimeout = 0

                    Dim dt As New DataTable()
                    dt.Load(cmd.ExecuteReader)
                    cmd.Dispose

                    ' Buffer dans un datatable
                    If dt.Rows.Count > 0 Then
                        Log(String.Format("{0} articles à mettre à jour", dt.Rows.Count))
                        For Each row As DataRow In dt.Rows
                            checkArticle(row)
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    'Champs à renseigner obligatoirement lors de l’ajout
    '   AR_Ref
    '   AR_Design
    '   FA_CodeFamille
    '   AR_UniteVen
    'Champs non modifiables en modification d’enregistrement
    '   AR_Ref
    '   AR_Type
    '   AR_PUNet
    '   AR_SuiviStock : Si mouvement
    '   AR_Gamme1 : Si article présent dans F_Docligne, F_Nomenclat, F_TarifGam ou F_ArtGamme
    '   AR_Gamme2 : Si article présent dans F_Docligne, F_Nomenclat, F_TarifGam ou F_ArtGamme
    '   AR_Condition : Si article présent dans F_ArtCond, F_TarifCond
    Private Function checkArticle(ByRef row As DataRow) As Boolean
        Dim updBlackList As New List(Of String) From {"AR_Ref", "AR_Type", "AR_PUNet", "AR_SuiviStock", "CO_No"}
        Dim arRef As String = row.Item("L_AR_Ref")
        Dim insert As Boolean = IsDBNull(row.Item("R_AR_Ref")) OrElse row.Item("R_AR_Ref").ToString = "0"
        Log(String.Format("{0} => {1} [{2}] {3}", dbSource.dbName, dbTarget.dbName , arRef, If(insert, "INSERT", "UPDATE")))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()

            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction
            Try
                Using cnxSource As New SqlConnection(dbSource.ToString)
                    cnxSource.Open()

                    '
                    ' F_ARTICLE
                    '
                    Using cmdSource As SqlCommand = GetSelectCmdFromCnx(
                        cnxSource,
                        "F_ARTICLE",
                        New List(Of String) From {"AR_Ref", "AR_Design", "FA_CodeFamille", "AR_Substitut", "AR_Raccourci", "AR_Garantie", "AR_UnitePoids", "AR_PoidsNet",
                            "AR_PoidsBrut", "AR_UniteVen", "AR_PrixAch", "AR_Coef", "AR_PrixVen", "AR_PrixTTC", "AR_Gamme1", "AR_Gamme2", "AR_Nomencl", "AR_Escompte",
                            "AR_Delai", "AR_HorsStat", "AR_VteDebit", "AR_NotImp", "AR_Sommeil", "AR_SuiviStock", "AR_Langue1", "AR_Langue2", "AR_EdiCode",
                            "AR_CodeBarre", "AR_CodeFiscal", "AR_Pays", "AR_Frais01FR_Denomination", "AR_Frais01FR_Rem01REM_Valeur", "AR_Frais01FR_Rem01REM_Type",
                            "AR_Frais01FR_Rem02REM_Valeur", "AR_Frais01FR_Rem02REM_Type", "AR_Frais01FR_Rem03REM_Valeur", "AR_Frais01FR_Rem03REM_Type", "AR_Frais02FR_Denomination",
                            "AR_Frais02FR_Rem01REM_Valeur", "AR_Frais02FR_Rem01REM_Type", "AR_Frais02FR_Rem02REM_Valeur", "AR_Frais02FR_Rem02REM_Type", "AR_Frais02FR_Rem03REM_Valeur",
                            "AR_Frais02FR_Rem03REM_Type", "AR_Frais03FR_Denomination", "AR_Frais03FR_Rem01REM_Valeur", "AR_Frais03FR_Rem01REM_Type", "AR_Frais03FR_Rem02REM_Valeur",
                            "AR_Frais03FR_Rem02REM_Type", "AR_Frais03FR_Rem03REM_Valeur", "AR_Frais03FR_Rem03REM_Type", "AR_Condition", "AR_PUNet", "AR_Contremarque", "AR_FactPoids",
                            "AR_FactForfait", "AR_SaisieVar", "AR_Transfere", "AR_Publie", "AR_Photo", "AR_PrixAchNouv", "AR_CoefNouv", "AR_PrixVenNouv", "AR_DateModif", "AR_DateCreation",
                            "AR_DateApplication", "AR_CoutStd", "AR_QteComp", "AR_QteOperatoire", "AR_Prevision", "CL_No1", "CL_No2", "CL_No3", "CL_No4", "AR_Type",
                            "RP_CodeDefaut", "AR_Nature", "AR_DelaiFabrication", "AR_NbColis", "AR_DelaiPeremption", "AR_DelaiSecurite", "AR_Fictif", "AR_SousTraitance", "AR_TypeLancement",
                            "SUPPRIME", "SUPPRIME_USINE"},
                        arRef
                    )

                        Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                            While reader.Read
                                ' Verif doublons codes barre
                                ' Codes barre F_ARTICLE
                                If reader.Item("AR_CodeBarre").ToString <> "" Then
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        With cmdTarget
                                            .Transaction = transaction
                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_ARTICLE] Set AR_CodeBarre = NULL WHERE AR_CodeBarre = @codeBarre And AR_Ref <> @ref", cnxTarget.Database)
                                            .Parameters.AddWithValue("@codeBarre", reader.Item("AR_CodeBarre"))
                                            .Parameters.AddWithValue("@ref", arRef)
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using
                                        
                                    ' Codes barres F_CONDITION
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        With cmdTarget
                                            .Transaction = transaction
                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_CONDITION] Set CO_CodeBarre = NULL WHERE CO_CodeBarre = @codeBarre And AR_Ref = @ref", cnxTarget.Database)
                                            .Parameters.AddWithValue("@codeBarre", reader.Item("AR_CodeBarre"))
                                            .Parameters.AddWithValue("@ref", arRef)
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using

                                    ' Codes barres F_TARIFGAM
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        With cmdTarget
                                            .Transaction = transaction
                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_TARIFGAM] Set TG_CodeBarre = NULL WHERE TG_CodeBarre = @codeBarre And AR_Ref = @ref", cnxTarget.Database)
                                            .Parameters.AddWithValue("@codeBarre", reader.Item("AR_CodeBarre"))
                                            .Parameters.AddWithValue("@ref", arRef)
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using
                                End If

                                If insert Then
                                    InsertRowFromReader("F_ARTICLE", reader, transaction)
                                Else
                                    ' Update
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        With cmdTarget
                                            .Transaction = transaction

                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_ARTICLE] SET ", cnxTarget.Database)
                                            For i As Integer = 0 To reader.FieldCount - 1
                                                If updBlackList.Contains(reader.GetName(i)) Then
                                                    Continue For
                                                End If
                                                .CommandText &= String.Format("[{0}] = @{0}", reader.GetName(i))
                                                .Parameters.AddWithValue(reader.GetName(i), reader.Item(i))

                                                If i <> reader.FieldCount - 1 Then
                                                    .CommandText &= ", "
                                                End If
                                            Next

                                            .CommandText &= " WHERE AR_Ref = @AR_Ref"
                                            .Parameters.AddWithValue("@AR_Ref", arRef)
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using
                                End If
                            End While
                        End Using
                    End Using
                    '
                    ' END F_ARTICLE
                    '

                    '
                    ' F_NOMENCLAT
                    '
                    ' DELETE NOMENCLATURES
                    Delete("F_NOMENCLAT", transaction, "AR_Ref", arRef)

                    '
                    ' F_TARIFGAM
                    ' Attention lien avec F_ARTICLE, F_ARTGAMME, F_ARTFOURNISS et F_ARTCLIENT
                    ' si diff, on supprime maintenant et recrée à la fin
                    Delete("F_TARIFGAM", transaction, "AR_Ref", arRef)

                    '
                    ' F_ARTENUMREF
                    ' Attention lien avec F_ARTICLE, F_ARTGAMME
                    ' si diff on supprime maintenant et on recrée à la fin
                    '
                    Delete("F_ARTENUMREF", transaction, "AR_Ref", arRef)

                    '
                    ' F_ARTGAMME
                    '
                    '
                    'Delete("F_ARTGAMME", transaction, "AR_Ref", arRef)
                    'CopySourceToTarget(cnxSource, "F_ARTGAMME", New List(Of String) From {"AR_Ref", "AG_No", "EG_Enumere", "AG_Type"}, arRef, transaction)
                    ' Impossible de delete + recréation des gammes si utilisées dans les docs
                    UpdateOrInsertArtGamme(cnxSource, transaction, arRef)

                    ' Recréation des Nomenclatures
                    CopySourceToTarget(
                        cnxSource,
                        "F_NOMENCLAT",
                        New List(Of String) From {"AR_Ref", "NO_RefDet", "NO_Qte", "AG_No1", "AG_No2", "NO_Type", "NO_Repartition",
                            "NO_Operation", "NO_Commentaire", "DE_No", "NO_Ordre", "AG_No1Comp", "AG_No2Comp", "NO_SousTraitance"},
                        arRef,
                        transaction
                    )

                    ' 
                    ' F_TARIFQTE & F_ARTCLIENT
                    '

                    ' DELETE COND
                    Delete("F_CONDITION", transaction, "AR_Ref", arRef)

                    ' DELETE TARIFS COND
                    Delete("F_TARIFCOND", transaction, "AR_Ref", arRef)

                    ' DELETE TARIF QTE
                    Delete("F_TARIFQTE", transaction, "AR_Ref", arRef)

                    ' DELETE CAT TARIFS
                    Delete("F_ARTCLIENT", transaction, "AR_Ref", arRef, "AC_Categorie <> 0")
                        
                    ' Recréation des CAT TARIFS
                    CopySourceToTarget(
                        cnxSource,
                        "F_ARTCLIENT",
                        New List(Of String) From {"AR_Ref", "AC_Categorie", "AC_PrixVen", "AC_Coef", "AC_PrixTTC", "AC_Arrondi", "AC_QteMont",
                            "EG_Champ", "AC_PrixDev", "AC_Devise", "CT_Num", "AC_Remise", "AC_Calcul", "AC_TypeRem", "AC_RefClient",
                            "AC_CoefNouv", "AC_PrixVenNouv", "AC_PrixDevNouv", "AC_RemiseNouv", "AC_DateApplication"},
                        arRef,
                        transaction,
                        "AC_Categorie <> 0"
                    )

                    ' Recréation des tarifs par quantite
                    CopySourceToTarget(
                        cnxSource,
                        "F_TARIFQTE",
                        New List(Of String) From {"AR_Ref", "TQ_RefCF", "TQ_BorneSup", "TQ_Remise01REM_Valeur", "TQ_Remise01REM_Type",
                            "TQ_Remise02REM_Valeur", "TQ_Remise02REM_Type", "TQ_Remise03REM_Valeur", "TQ_Remise03REM_Type", "TQ_PrixNet"},
                        arRef,
                        transaction
                    )
                        
                    ' CONDITIONNEMENT
                    ' Supprime les CO_No pouvant être en conflit de clé unique
                    Using cmdSource As SqlCommand = GetSelectCmdFromCnx(cnxSource, "F_CONDITION", New List(Of String) From {"AR_Ref", "CO_No"}, arRef)
                        Using reader As SqlDataReader = cmdSource.ExecuteReader
                            While reader.Read()
                                Delete("F_CONDITION", transaction, "CO_No", reader.Item("CO_No"))
                            End While
                        End Using
                    End Using

                    ' Recréation des tarifs cond
                    CopySourceToTarget(cnxSource, "F_CONDITION", New List(Of String) From {"AR_Ref", "CO_No", "EC_Enumere", "EC_Quantite", "CO_Ref", "CO_CodeBarre", "CO_Principal"}, arRef, transaction)
                        
                    ' Update le CO_No de l'article une fois que tout les F_CONDITION sont créés/supprimé
                    Using cmdSource As SqlCommand = GetSelectCmdFromCnx(cnxSource, "F_ARTICLE", New List(Of String) From {"AR_Ref", "CO_No"}, arRef)
                        Using reader As SqlDataReader = cmdSource.ExecuteReader
                            reader.Read

                            Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                With cmdTarget
                                    .Transaction = transaction
                                    .CommandText = String.Format("UPDATE {0}.[dbo].[F_ARTICLE] SET CO_No = @CO_No WHERE AR_Ref = @AR_Ref", cnxTarget.Database)
                                    .Parameters.AddWithValue("@CO_No", reader.Item("CO_No"))
                                    .Parameters.AddWithValue("@AR_Ref", arRef)
                                    .CommandTimeout = cmdTimeOut
                                    Log(.CommandText)
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End Using
                    End Using

                    CopySourceToTarget(cnxSource, "F_TARIFCOND", New List(Of String) From {"AR_Ref", "TC_RefCF", "CO_No", "TC_Prix", "TC_PrixNouv"}, arRef, transaction)

                    '
                    ' F_ARTFOURNISS
                    '
                    'If Not AreIdenticals(viewArt.Item("L_csArtFournissAgg"), viewArt.Item("R_csArtFournissAgg")) Then
                        ' DELETE TARIF(S) FOURNISSEUR
                        Delete("F_ARTFOURNISS", transaction, "AR_Ref", arRef)

                        ' Recréation des tarifs fournisseurs
                        Using cmdSource As SqlCommand = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_ARTFOURNISS",
                            New List(Of String) From {"AR_Ref", "CT_Num", "AF_RefFourniss", "AF_PrixAch", "AF_Unite", "AF_Conversion", "AF_DelaiAppro", "AF_Garantie",
                                "AF_Colisage", "AF_QteMini", "AF_QteMont", "EG_Champ", "AF_Principal", "AF_PrixDev", "AF_Devise", "AF_Remise", "AF_ConvDiv", "AF_TypeRem",
                                "AF_CodeBarre", "AF_PrixAchNouv", "AF_PrixDevNouv", "AF_RemiseNouv", "AF_DateApplication"},
                            arRef
                        )

                            Using reader As SqlDataReader = cmdSource.ExecuteReader
                                If reader.HasRows Then
                                    While reader.Read
                                        ' Verif code barre
                                        If reader.Item("AF_CodeBarre").ToString <> "" Then
                                            Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                                With cmdTarget
                                                    .Transaction = transaction
                                                    .CommandText = String.Format("UPDATE {0}.[dbo].[F_ARTFOURNISS] Set AF_CodeBarre = NULL WHERE AF_CodeBarre = @codeBarre AND AR_Ref <> @ref AND CT_Num = @ctNum", cnxTarget.Database)
                                                    .Parameters.AddWithValue("@codeBarre", reader.Item("AF_CodeBarre"))
                                                    .Parameters.AddWithValue("@ref", reader.Item("AR_Ref"))
                                                    .Parameters.AddWithValue("@ctNum", reader.Item("CT_Num"))
                                                    .CommandTimeout = cmdTimeOut
                                                    .ExecuteNonQuery()
                                                End With
                                            End Using
                                        End If

                                        ' Verif Ref Fourn
                                        If reader.Item("AF_RefFourniss").ToString <> "" Then
                                            Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                                With cmdTarget
                                                    .Transaction = transaction
                                                    .CommandText = String.Format("UPDATE {0}.[dbo].[F_ARTFOURNISS] Set AF_RefFourniss = NULL WHERE AF_RefFourniss = @refFourn AND AR_Ref <> @ref And CT_Num = @ctNum", cnxTarget.Database)
                                                    .Parameters.AddWithValue("@refFourn", reader.Item("AF_RefFourniss"))
                                                    .Parameters.AddWithValue("@ref", reader.Item("AR_Ref"))
                                                    .Parameters.AddWithValue("@ctNum", reader.Item("CT_Num"))
                                                    .CommandTimeout = cmdTimeOut
                                                    .ExecuteNonQuery()
                                                End With
                                            End Using
                                        End If

                                        InsertRowFromReader("F_ARTFOURNISS", reader, transaction)
                                    End While
                                End If
                            End Using
                        End Using
                    'End If
                    '
                    ' END F_ARTFOURNISS
                    '

                    '
                    ' F_ARTCOMPTA
                    '
                    ' DELETE PASSERELLES COMPTA
                    Delete("F_ARTCOMPTA", transaction, "AR_Ref", arRef)

                    ' Recréation des passerelles comptables
                    CopySourceToTarget(
                        cnxSource,
                        "F_ARTCOMPTA",
                        New List(Of String) From {"AR_Ref", "ACP_Type", "ACP_Champ", "ACP_ComptaCPT_CompteG", "ACP_ComptaCPT_CompteA",
                            "ACP_ComptaCPT_Taxe1", "ACP_ComptaCPT_Taxe2", "ACP_ComptaCPT_Taxe3", "ACP_ComptaCPT_Date1", "ACP_ComptaCPT_Date2",
                            "ACP_ComptaCPT_Date3", "ACP_ComptaCPT_TaxeAnc1", "ACP_ComptaCPT_TaxeAnc2", "ACP_ComptaCPT_TaxeAnc3", "ACP_TypeFacture"},
                        arRef,
                        transaction
                    )

                    '
                    ' F_ARTENUMREF
                    '
                    ' Les tarifs gammes ont déjà été supprimés plus haut, on les recrée juste
                    CopySourceToTarget(cnxSource, "F_ARTENUMREF", New List(Of String) From {"AR_Ref", "AG_No1", "AG_No2", "AE_Ref", "AE_PrixAch", "AE_CodeBarre", "AE_PrixAchNouv", "AE_EdiCode", "AE_Sommeil"}, arRef, transaction)

                    '
                    ' F_TARIFGAM
                    '
                    ' Les tarifs gammes ont déjà été supprimés plus haut, on les recrée juste
                    CopySourceToTarget(cnxSource, "F_TARIFGAM", New List(Of String) From {"AR_Ref", "TG_RefCF", "AG_No1", "AG_No2", "TG_Prix", "TG_Ref", "TG_CodeBarre", "TG_PrixNouv"}, arRef, transaction)

                End Using ' END cnxsource

                transaction.Commit()
                Return True
            Catch ex As Exception
                Log(String.Format("Commit Exception: {0}", ex.ToString))

                ' Attempt to roll back the transaction. 
                Try
                    transaction.Rollback()
                    Return False
                Catch ex2 As Exception
                    ' This catch block will handle any errors that may have occurred 
                    ' on the server that would cause the rollback to fail, such as 
                    ' a closed connection.
                    Log(String.Format("Rollback Exception Type: {0}", ex2.ToString))
                    Return False
                End Try
            End Try
        End Using
    End Function

    Protected Sub UpdateOrInsertArtGamme(ByRef cnxSource As SqlConnection, ByRef transaction As SqlTransaction, ByVal arRef As String)
        Using cmdSource As SqlCommand = GetSelectCmdFromCnx(cnxSource, "F_ARTGAMME", New List(Of String) From {"AR_Ref", "AG_No", "EG_Enumere", "AG_Type"}, arRef)
            Using reader As SqlDataReader = cmdSource.ExecuteReader
                If reader.HasRows
                    While reader.Read
                        Using cmdTarget As SqlCommand = transaction.Connection.CreateCommand
                            With cmdTarget
                                .Transaction = transaction
                                .CommandText = String.Format(
                                    "UPDATE {0}.[dbo].[F_ARTGAMME] SET EG_Enumere = @egEnum, AG_Type = @agType WHERE AR_Ref = @arRef AND AG_No = @agNo
                                     IF @@ROWCOUNT=0
                                        INSERT INTO {0}.[dbo].[F_ARTGAMME] (AR_Ref, AG_No, EG_Enumere, AG_Type) VALUES(@arRef, @agNo, @egEnum, @agType)",
                                    transaction.Connection.Database
                                )
                                .Parameters.AddWithValue("@arRef", arRef)
                                .Parameters.AddWithValue("@agNo", reader.Item("AG_No"))
                                .Parameters.AddWithValue("@egEnum", reader.Item("EG_Enumere"))
                                .Parameters.AddWithValue("@agType", reader.Item("AG_Type"))
                                .CommandTimeout = cmdTimeOut
                                Log(.CommandText)
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End While
                End If
            End Using
        End Using
    End Sub
End Class
