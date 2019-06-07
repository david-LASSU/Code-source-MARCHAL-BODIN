Imports System.Data.SqlClient
Imports System

Public Class FournisseurTool
    Inherits Tool

    ' List of FOURNCHKSYNCVIEW.COLUMNS
    Private vCols As New List(Of String) From {"CT_Num", "csTiers", "CS", "csContactTAgg", "csBanqueTAgg", "csReglementTAgg", "ctNumPos"}

    Public Sub New(ByRef dbTarget As Database, ByRef dbSource As Database)
        MyBase.New(dbTarget, dbSource)
    End Sub

    Public Sub MajFourn()
        Log("[MAJ FOURN]")
        Try
            Using cnxSource As New SqlConnection(dbSource.ToString)
                Using cmd As SqlCommand = cnxSource.CreateCommand
                    cnxSource.Open()
                    ' Requete ~= 12 secondes Récup les différences entre les bases
                    cmd.CommandText = String.Format(
                        "SELECT {0} 
                        FROM {1}.[FOURNCHKSYNCVIEW] L 
                        LEFT JOIN {2}.[FOURNCHKSYNCVIEW] R On R.CT_Num = L.CT_Num
                        WHERE L.CS <> R.CS Or R.CT_Num Is NULL
                        ORDER BY L.ctNumPos, L.CT_Num",
                        GetViewSelectAliases(vCols),
                        dbSource.sqlDbName,
                        dbTarget.sqlDbName
                    )
                    cmd.CommandTimeout = cmdTimeOut

                    Dim dt As New DataTable()
                    dt.Load(cmd.ExecuteReader)
                    cmd.Dispose()

                    ' Buffer dans un datatable
                    If dt.Rows.Count > 0 Then
                        Log(String.Format("{0} fournisseurs à mettre à jour", dt.Rows.Count))
                        For Each row As DataRow In dt.Rows
                            checkFourn(row)
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    'Champs à renseigner obligatoirement lors de l’ajout
    '
    'CT_Num
    'CT_Intitule
    'CT_Type
    'CG_NumPrinc
    'CT_NumPayeur
    'N_Risque
    'N_CatTarif
    'N_CatCompta
    'N_Period
    'N_Expedition
    'N_Condition
    '
    'Champs non modifiables en modification d’enregistrement
    '
    'CT_Num
    'CT_Type
    'CT_DateCreate
    'N_Analytique
    Private Function checkFourn(ByRef viewFourn As DataRow) As Boolean
        Dim ctNum As String = viewFourn.Item("L_CT_Num")
        Dim insert As Boolean = IsDBNull(viewFourn.Item("R_CT_Num")) OrElse viewFourn.Item("R_CT_Num").ToString = "0"

        ' Liste des champs à exclure de l'update
        ' Le BT_Num sera géré après la création des banques
        Dim updBlackList As New List(Of String) From {"BT_Num", "CT_Num", "CT_Type", "CT_DateCreate", "N_Analytique"}
        ' Start transaction

        Log(String.Format("[{0}] {1}", ctNum, If(insert, "INSERT", "UPDATE")))

        Using cnxTarget As New SqlConnection(dbTarget.ToString)
            cnxTarget.Open()

            Dim transaction As SqlTransaction = cnxTarget.BeginTransaction
            Try

                Using cnxSource As New SqlConnection(dbSource.ToString)
                    cnxSource.Open()

                    '
                    ' F_COMPTET
                    '
                    Using cmdSource As SqlCommand = GetSelectCmdFromCnx(
                        cnxSource,
                        "F_COMPTET",
                        New List(Of String) From {"CT_Num", "CT_Intitule", "CT_Type", "CG_NumPrinc", "CT_Qualite", "CT_Classement", "CT_Contact", "CT_Adresse", "CT_Complement", "CT_CodePostal",
                            "CT_Ville", "CT_CodeRegion", "CT_Pays", "CT_Raccourci", "N_Devise", "CT_Ape", "CT_Identifiant", "CT_Siret", "CT_Commentaire", "CT_Encours",
                            "CT_Assurance", "CT_NumPayeur", "N_Risque", "N_CatTarif", "CT_Taux01", "CT_Taux02", "CT_Taux03", "CT_Taux04", "N_CatCompta", "N_Period",
                            "CT_Facture", "CT_BLFact", "CT_Langue", "N_Expedition", "N_Condition", "CT_DateCreate", "CT_Saut", "CT_Lettrage", "CT_ValidEch", "CT_Sommeil", "DE_No",
                            "CT_ControlEnc", "CT_NotRappel", "N_Analytique", "CA_Num", "CT_Telephone", "CT_Telecopie", "CT_EMail", "CT_Site", "CT_Coface", "CT_Surveillance",
                            "CT_SvDateCreate", "CT_SvFormeJuri", "CT_SvEffectif", "CT_SvCA", "CT_SvResultat", "CT_SvIncident", "CT_SvDateIncid", "CT_SvPrivil", "CT_SvRegul",
                            "CT_SvCotation", "CT_SvDateMaj", "CT_SvObjetMaj", "CT_SvDateBilan", "CT_SvNbMoisBilan", "CT_PrioriteLivr", "CT_LivrPartielle", "MR_No", "CT_NotPenal",
                            "EB_No", "CT_NumCentrale", "CT_DateFermeDebut", "CT_DateFermeFin", "CT_FactureElec", "CT_TypeNIF", "CT_RepresentInt", "CT_RepresentNIF", "Identifiant",
                            "1", "2", "3", "4", "5", "6", "7", "8", "9", "GESTION DES RELIQUATS", "CT_EdiCodeType", "CT_EdiCode", "CT_EdiCodeSage", "CT_ProfilSoc", "N° de compte",
                            "CT_StatutContrat", "CT_DateMAJ", "CT_EchangeRappro", "CT_EchangeCR", "PI_NoEchange", "CT_BonAPayer", "CT_DelaiTransport", "CT_DelaiAppro", "INFO_MAJ_TARIF",
                            "CT_LangueISO2", "Pratique escompte", "DATE_MAJ_TARIF", "FRANCO", "MAJ_URGENTE", "EN_COURS_DE_MAJ", "magasin_referent", "magasin_livraison", "MAJ_PX_VENTE_A_DATE"},
                        ctNum
                        )
                        Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                            While reader.Read
                                If insert Then
                                    InsertRowFromReader("F_COMPTET", reader, transaction)
                                Else
                                    ' Update
                                    Dim cleanInfoName As String
                                    Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                        With cmdTarget
                                            .Transaction = transaction
                                            .CommandText = String.Format("UPDATE {0}.[dbo].[F_COMPTET] SET ", cnxTarget.Database)
                                            For i As Integer = 0 To reader.FieldCount - 1
                                                If updBlackList.Contains(reader.GetName(i)) Then
                                                    Continue For
                                                End If

                                                cleanInfoName = reader.GetName(i).Replace(" ", "").Replace("°", "")
                                                .CommandText &= String.Format("[{0}] = @{1}", reader.GetName(i), cleanInfoName)
                                                .Parameters.AddWithValue(cleanInfoName, reader.Item(i))

                                                If i <> reader.FieldCount - 1 Then
                                                    .CommandText &= ", "
                                                End If
                                            Next
                                            .CommandText &= " WHERE CT_Num = @CT_Num"
                                            .Parameters.AddWithValue("@CT_Num", ctNum)
                                            .CommandTimeout = cmdTimeOut
                                            .ExecuteNonQuery()
                                        End With
                                    End Using
                                End If
                            End While
                        End Using
                    End Using
                    '
                    ' END F_COMPTET
                    '

                    '
                    ' F_CONTACTT
                    '
                    'Champs à renseigner obligatoirement lors de l’ajout
                    '
                    'CT_Num
                    'CT_Nom
                    'N_Service
                    'N_Contact
                    Delete("F_CONTACTT", transaction, "CT_Num", ctNum)
                    CopySourceToTarget(
                        cnxSource,
                        "F_CONTACTT",
                        New List(Of String) From {"CT_Num", "CT_Nom", "CT_Prenom", "N_Service", "CT_Fonction", "CT_Telephone", "CT_TelPortable", "CT_Telecopie", "CT_EMail", "CT_Civilite", "N_Contact"},
                        ctNum,
                        transaction
                    )

                    '
                    ' F_BANQUET
                    '
                    Delete("F_BANQUET", transaction, "CT_Num", ctNum)
                    CopySourceToTarget(
                        cnxSource,
                        "F_BANQUET",
                        New List(Of String) From {"CT_Num", "BT_Num", "BT_Intitule", "BT_Banque", "BT_Guichet", "BT_Compte", "BT_Cle", "BT_Commentaire", "BT_Struct", "N_Devise", "BT_Adresse", "BT_Complement", "BT_CodePostal", "BT_Ville", "BT_Pays", "BT_BIC", "BT_IBAN", "BT_CalculIBAN", "BT_NomAgence", "BT_CodeRegion", "BT_PaysAgence", "MD_No"},
                        ctNum,
                        transaction
                    )

                    ' Met à jour la banque principale
                    Using cmdSource As SqlCommand = cnxSource.CreateCommand
                        cmdSource.CommandText = String.Format("SELECT [BT_Num] FROM {0}.[F_COMPTET] WHERE CT_Num = @ctNum", dbSource.sqlDbName)
                        cmdSource.Parameters.AddWithValue("@ctNum", ctNum)
                        cmdSource.CommandTimeout = cmdTimeOut

                        Using reader As SqlDataReader = cmdSource.ExecuteReader(CommandBehavior.SingleRow)
                            While reader.Read
                                Using cmdTarget As SqlCommand = cnxTarget.CreateCommand
                                    With cmdTarget
                                        .Transaction = transaction
                                        .CommandText = String.Format("UPDATE {0}.[dbo].[F_COMPTET] Set [BT_Num] = @btNum WHERE [CT_Num] = @ctNum", cnxTarget.Database)
                                        .Parameters.AddWithValue("@btNum", reader.Item("BT_Num"))
                                        .Parameters.AddWithValue("@ctNum", ctNum)
                                        .CommandTimeout = cmdTimeOut
                                        .ExecuteNonQuery()
                                    End With
                                End Using
                            End While
                        End Using
                    End Using

                    '
                    ' F_REGLEMENTT
                    '
                    Delete("F_REGLEMENTT", transaction, "CT_Num", ctNum)
                    CopySourceToTarget(
                        cnxSource,
                        "F_REGLEMENTT",
                        New List(Of String) From {"CT_Num", "N_Reglement", "RT_Condition", "RT_NbJour", "RT_JourTb01", "RT_JourTb02", "RT_JourTb03", "RT_JourTb04", "RT_JourTb05", "RT_JourTb06", "RT_TRepart", "RT_VRepart"},
                        ctNum,
                        transaction
                    )
                End Using ' End cnxSource

                transaction.Commit()
                Return True

            Catch ex As Exception
                Log(String.Format("Commit Exception : {0}", ex.ToString))

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
End Class
