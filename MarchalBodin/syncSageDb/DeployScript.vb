Imports System.IO
Imports System.Text
Imports System.Security
Imports System.Security.Cryptography
Imports System.Data.SqlClient
Imports Microsoft.SqlServer.Management.Smo
Imports Microsoft.SqlServer.Management.Common

Public Class DeployScript
    Inherits Tool

    Public Sub New(ByRef dbTarget As Database)
        MyBase.New(dbTarget)
    End Sub

    Public Sub deploySqlScripts()
        Try
            Using cnxTarget As New SqlConnection(dbTarget.ToString)
                Log(String.Format("Deploy scripts {0} ...", cnxTarget.Database))
                cnxTarget.Open()
                'Rep contenant les fichiers de hash des script SQL
                'Dim depDir As String = "\\SRVAD01\gestion\Programmes externes\DeployScripts\"

#If DEBUG Then
                Dim sqlDir As New DirectoryInfo("\\SRVAD01\COMMUN\SUPPORT\SQL\ScriptsDev")
#Else
                Dim sqlDir As New DirectoryInfo(String.Format("\\{0}\COMMUN\SUPPORT\SQL\ScriptsProd", Replace(dbTarget.hostname, "SRVSQL", "SRVAD")))
#End If
                Dim sqlFiles As FileInfo() = sqlDir.GetFiles()
                For Each sqlFile As FileInfo In sqlFiles
                    If Left(sqlFile.Name, 1) = "_" Then
                        Log(String.Format("{0} :: Exclusion du fichier {1}", cnxTarget.Database, sqlFile.Name))
                        Continue For
                    End If
                    If sqlFile.Extension = ".sql" Then
                        ' Hash du fichier courant
                        Dim hash As String = sha_256(sqlFile.FullName)
                        ' Fichier texte contenant l'ancien hash
                        Dim depFileName As String = depDir + "\" + cnxTarget.Database + "_" + Replace(sqlFile.Name, ".sql", ".txt")

                        Dim oldHash As String = String.Empty
                        Using fs = File.Open(depFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)
                            Dim b(1024) As Byte
                            Do While fs.Read(b, 0, b.Length) > 0
                                oldHash = New UTF8Encoding(True).GetString(b)
                            Loop
                        End Using

                        ' Les deux hashs sont différents ou le nom de fichier commence par un ~ , on integre le nouveau script SQL
                        If String.Compare(hash, oldHash) <> 0 Or Left(sqlFile.Name, 1) = "~" Then
                            Log(String.Format("{0} :: Intégration du fichier {1}", cnxTarget.Database, sqlFile.Name))
                            Try
                                ' Création
                                Dim file As New FileInfo(sqlFile.FullName)
                                Dim srv As New Server(New ServerConnection(cnxTarget))
                                srv.ConnectionContext.ExecuteNonQuery(file.OpenText().ReadToEnd())

                                ' Met à jour le fichier avec le nouveau hash
                                Using fw As StreamWriter = New StreamWriter(depFileName, False)
                                    fw.Write(hash)
                                End Using
                            Catch ex As Exception
                                Log(ex.ToString)
                            End Try
                        End If
                    End If
                Next
            End Using
        Catch ex As Exception
            Log(ex.ToString)
        End Try
    End Sub

    ' Function to obtain the desired hash of a file
    Function hash_generator(ByVal hash_type As String, ByVal file_name As String)

        ' We declare the variable : hash
        Dim hash
        If hash_type.ToLower = "md5" Then
            ' Initializes a md5 hash object
            hash = MD5.Create
        ElseIf hash_type.ToLower = "sha1" Then
            ' Initializes a SHA-1 hash object
            hash = SHA1.Create()
        ElseIf hash_type.ToLower = "sha256" Then
            ' Initializes a SHA-256 hash object
            hash = SHA256.Create()
        Else
            MsgBox("Unknown type of hash : " & hash_type, MsgBoxStyle.Critical)
            Return False
        End If

        ' We declare a variable to be an array of bytes
        Dim hashValue() As Byte

        ' We create a FileStream for the file passed as a parameter
        Dim fileStream As FileStream = File.OpenRead(file_name)
        ' We position the cursor at the beginning of stream
        fileStream.Position = 0
        ' We calculate the hash of the file
        hashValue = hash.ComputeHash(fileStream)
        ' The array of bytes is converted into hexadecimal before it can be read easily
        Dim hash_hex = PrintByteArray(hashValue)

        ' We close the open file
        fileStream.Close()

        ' The hash is returned
        Return hash_hex

    End Function

    ' We traverse the array of bytes and converting each byte in hexadecimal
    Public Function PrintByteArray(ByVal array() As Byte)

        Dim hex_value As String = ""

        ' We traverse the array of bytes
        Dim i As Integer
        For i = 0 To array.Length - 1

            ' We convert each byte in hexadecimal
            hex_value += array(i).ToString("X2")

        Next i

        ' We return the string in lowercase
        Return hex_value.ToLower

    End Function

    ' md5 is a reserved name, so we named the function : md5_hash
    Function md5_hash(ByVal file_name As String)
        Return hash_generator("md5", file_name)
    End Function

    Function sha_1(ByVal file_name As String)
        Return hash_generator("sha1", file_name)
    End Function

    Function sha_256(ByVal file_name As String)
        Return hash_generator("sha256", file_name)
    End Function

End Class
