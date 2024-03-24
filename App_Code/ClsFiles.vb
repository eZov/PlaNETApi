Imports System.IO
Imports System.Security.Cryptography
Imports Microsoft.VisualBasic

Public Class ClsFiles

    Public Shared Function CreateMD5(ByVal pFilePathName As String) As String
        'accessing file & getting the hash
        Try
            Dim RD As FileStream
            RD = New FileStream(pFilePathName, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
            Dim md5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider
            md5.ComputeHash(RD)
            RD.Close()

            'converting the bytes into string
            Dim hash As Byte() = md5.Hash
            Dim SB As StringBuilder = New StringBuilder
            Dim HB As Byte
            For Each HB In hash
                SB.Append(String.Format("{0:X2}", HB))
            Next
            Return SB.ToString()


        Catch ex As Exception

        End Try

        Return Nothing

    End Function

End Class
