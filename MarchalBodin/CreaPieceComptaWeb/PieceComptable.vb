Imports Objets100cLib
Imports System.Text
Imports System.Data
Imports System.Data.SqlClient

Module PieceComptable

    Function pieceExiste(ByRef conn As SqlClient.SqlConnection, ByVal ref As String) As Boolean
        Dim cmd As SqlCommand = conn.CreateCommand
        cmd.CommandText = "SELECT * FROM F_ECRITUREC WHERE EC_Reference = @ref"
        cmd.Parameters.AddWithValue("@ref", ref)
        If cmd.ExecuteScalar() = Nothing Then
            Return False
        End If

        Return True
    End Function

    Function CreePieceComptable(ByRef OM_BaseCpta As BSCPTAApplication100c,
                                ByVal codeJournal As String,
                                ByVal dateEcriture As Date,
                                Optional ByVal numPiece As String = "",
                                Optional ByVal reference As String = "",
                                Optional ByVal refPiece As String = "") As IPMEncoder
        Try

            Dim OM_PieceComptable As IPMEncoder = OM_BaseCpta.CreateProcess_Encoder

            OM_PieceComptable.bAnalytiqueAuto = False
            OM_PieceComptable.Date = dateEcriture
            OM_PieceComptable.Journal = OM_BaseCpta.FactoryJournal.ReadNumero(codeJournal)

            If numPiece = String.Empty Then
                OM_PieceComptable.EC_Piece = OM_PieceComptable.Journal.NextEC_Piece(dateEcriture)
            Else
                OM_PieceComptable.EC_Piece = numPiece
            End If

            If reference = String.Empty Then
                OM_PieceComptable.EC_Reference = Nothing
            Else
                OM_PieceComptable.EC_Reference = reference
            End If

            If refPiece = String.Empty Then
                OM_PieceComptable.EC_RefPiece = Nothing
            Else
                OM_PieceComptable.EC_RefPiece = refPiece
            End If

            Return OM_PieceComptable

        Catch ex As Exception
            Throw New Exception("Erreur en création d'une pièce comptable sur le journal " & codeJournal & " à la date du " & dateEcriture.ToString("dd/mm/yy") & " : " & ex.Message)
        End Try
    End Function

    Function AjouteLigneEcritureGenerale(ByRef OM_PieceComptable As IPMEncoder,
                                         ByVal numCpteGeneral As String,
                                         ByVal numCpteTiers As String,
                                         ByVal numCpteGeneralContrepartie As String,
                                         ByVal intitule As String,
                                         ByVal OM_SensEcriture As EcritureSensType,
                                         ByVal montant As Double,
                                         ByVal intitModeReglement As String,
                                         Optional ByVal dateEcheance As Date = Nothing) As IBOEcriture3

        Try

            Dim OM_BaseCpta As BSCPTAApplication100c = OM_PieceComptable.Journal.Stream
            Dim OM_EcritureGenerale As IBOEcriture3 = OM_PieceComptable.FactoryEcritureIn.Create

            If numCpteGeneral = String.Empty Then
                OM_EcritureGenerale.CompteG = Nothing
            Else
                OM_EcritureGenerale.CompteG = OM_BaseCpta.FactoryCompteG.ReadNumero(numCpteGeneral)
            End If

            If numCpteTiers = String.Empty Then
                OM_EcritureGenerale.Tiers = Nothing
            Else
                OM_EcritureGenerale.Tiers = OM_BaseCpta.FactoryTiers.ReadNumero(numCpteTiers)
            End If

            If numCpteGeneralContrepartie = String.Empty Then
                OM_EcritureGenerale.CompteGContrepartie = Nothing
            Else
                OM_EcritureGenerale.CompteGContrepartie = OM_BaseCpta.FactoryCompteG.ReadNumero(numCpteGeneralContrepartie)
            End If

            OM_EcritureGenerale.EC_Intitule = intitule
            OM_EcritureGenerale.EC_Echeance = dateEcheance
            OM_EcritureGenerale.EC_Sens = OM_SensEcriture
            OM_EcritureGenerale.EC_Montant = montant

            If intitModeReglement = String.Empty Then
                OM_EcritureGenerale.Reglement = Nothing
            Else
                OM_EcritureGenerale.Reglement = OM_BaseCpta.FactoryReglement.ReadIntitule(intitModeReglement)
            End If

            OM_EcritureGenerale.Write()

            Return OM_EcritureGenerale

        Catch ex As Exception
            Throw New Exception("Erreur en ajout d'une écriture au journal " & OM_PieceComptable.Journal.JO_Num & " à la date du " & OM_PieceComptable.Date.ToString("dd/MM/yy") & " : " & ex.Message)
        End Try

    End Function

    Sub ValidePieceComptable(ByRef pieceComptable As IPMEncoder)

        Try
            If pieceComptable.CanProcess Then
                pieceComptable.Process()

            Else
                Throw New Exception("Validation de la pièce comptable impossible : ")

            End If

        Catch ex As Exception

            Dim erreurs As New StringBuilder(ex.Message)

            For Each erreur As IFailInfo In pieceComptable.Errors
                erreurs.AppendLine(erreur.Text)
            Next

            Throw New Exception(erreurs.ToString)

        End Try

    End Sub
End Module
