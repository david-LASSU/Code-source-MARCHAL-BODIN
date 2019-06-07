Imports System.IO
Imports System.Data
Imports System.Data.SqlClient

Module Module1
    ' Hostname
    Public hostname As String = System.Net.Dns.GetHostName()
    ' Répertoire de log
    Private logDir As String = "C:\\SyncSageDb\Logs"
    Private logDate As String = Date.Now.ToString("yyyy-MM-dd")
    ' Répertoire de deploiement  des scripts
    Public depDir As String = "C:\\SyncSageDb\DeployScripts"

    ' TimeOut Global pour toutes les commandes SQL (en secondes)
    Public Const cmdTimeOut As Integer = 240

    Sub Main(args As String())
        EventLog.WriteEntry("Application", "START SYNC SAGE DB")
        Debug.Print("GetCommandLineArgs: {0}", String.Join(", ", args))

        Try
            ' Crée le rep de logs
            If (Not System.IO.Directory.Exists(logDir)) Then
                System.IO.Directory.CreateDirectory(logDir)
            End If

            If (Not System.IO.Directory.Exists(depDir)) Then
                System.IO.Directory.CreateDirectory(depDir)
            End If

            ' Vidage des vieux fichiers de log
            For Each oldfile As String In (From file In New IO.DirectoryInfo(logDir).GetFiles("*.log", IO.SearchOption.TopDirectoryOnly) Where file.LastWriteTime < Date.Now.AddDays(-7) Select file.FullName).ToArray
                IO.File.Delete(oldfile)
            Next

            ' Base source
            Dim sourceDb As New Database(args(0).Split(".").Last & ".MARCHAL-BODIN.LOCAL", args(0).Split(".").First)

            ' Si un seul argument, c'est un deployScript sur la source
            If args.Count = 1 Then
                With New DeployScript(sourceDb)
                    clearSessions(sourceDb)
                    .deploySqlScripts()
                    Return
                End With
            End If

            ' Bases à update
            Dim targetDbs As New List(Of Database)
            For Each d In args
                If Not Left(d, 5) = "TARIF" Then
                    targetDbs.Add(New Database(d.Split(".").Last & ".MARCHAL-BODIN.LOCAL", d.Split(".").First))
                End If
            Next

            ' Synchronise les bases société
            For Each targetDb In targetDbs
                Try
                    Log("[START SYNC]", targetDb)

                    ' Erreur à Langon instruction DBCC incorrect
                    If Not clearSessions(targetDb) Then
                        Continue For
                    End If

                    With New DeployScript(targetDb)
                        .deploySqlScripts()
                    End With

                    ' BASES SOCIETE VERS TARIF
                    ' Maj codes barres
#If Not DEBUG
                    With New ArticleTool(sourceDb, targetDb)
                        .MajCodesBarres()
                    End With
#End If                    

                    ' BASE TARIF VERS SOCIETE

                    ' Crée les comptes comptables
                    With New ComptaTool(targetDb, sourceDb)
                        .MajCompta()
                    End With

                    ' Maj fourn
                    With New FournisseurTool(targetDb, sourceDb)
                        .MajFourn()
                    End With

                    ' Crée les nouvelles catégories ou met à jour
                    With New CatalogueTool(targetDb, sourceDb)
                        .CreateOrUpdateCatalogue()
                    End With

                    ' Maj familles
                    With New FamilleTool(targetDb, sourceDb)
                        .MajFamilles()
                    End With

                    ' Maj Modèles de reglement
                    ' TODO résoudre les problèmes d'index entre les bases avant MEP
                    'With New ModeleRegTool(cnxSource, cnxTarget)
                    '    .MajModReg()
                    'End With

                    ' Maj articles
                    With New ArticleTool(targetDb, sourceDb)
                        .MajConditionnements()
                        .MajGammes()
                        .MajArticles()
                    End With

                    With New GlossaireTool(targetDb, sourceDb)
                        .MajGlossaire()
                        .DeleteGlossaire()
                    End With

                    ' Supprime les éventuelles familles
                    With New FamilleTool(targetDb, sourceDb)
                        .DeleteFamille()
                    End With

                    ' Supprime les éventuelles catégories
                    With New CatalogueTool(targetDb, sourceDb)
                        .DeleteCatalogue()
                    End With

                    Log("[END SYNC]", targetDb)
                Catch ex As Exception
                    Log(ex.ToString, targetDb)
                End Try
            Next
        Catch ex As Exception
            Console.Write(ex.ToString)
            EventLog.WriteEntry("Application", ex.ToString)
            'Console.ReadKey
        Finally
            EventLog.WriteEntry("Application", "END SYNC SAGE DB")
        End Try
    End Sub

    Private Function clearSessions(ByRef db As Database) As Boolean
        Try
            Using cnxTarget As New SqlConnection(db.ToString)
                Using cmd As SqlCommand = cnxTarget.CreateCommand
                    cnxTarget.Open()
                    With cmd
                        .CommandText = "DBCC cbsqlxp(free);
                                        DELETE FROM cbMessage;
                                        DELETE FROM cbNotification;
                                        DELETE FROM cbRegfile;
                                        DELETE FROM cbRegMessage;
                                        DELETE FROM cbRegUser;
                                        DELETE FROM cbUserSession;"
                        .CommandTimeout = cmdTimeOut
                        .ExecuteNonQuery()
                    End With
                End Using
            End Using
            Return True
        Catch ex As Exception
            Log(ex.ToString, db)
            Return False
        End Try
    End Function

    Public Sub Log(logMessage As String, db As Database)
        Dim logFile As String = String.Format("{0}\{1}-{2}.log", logDir, logDate, db.dbName)
        Debug.Print(logMessage)
        Console.WriteLine(logMessage)
        Using w As StreamWriter = File.AppendText(logFile)
            w.WriteLine("{0} {1} : {2}", DateTime.Now.ToShortDateString, DateTime.Now.ToShortTimeString, logMessage)
        End Using
    End Sub
End Module
