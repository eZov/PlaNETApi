Imports System.Data
Imports System.IO
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports Microsoft.VisualBasic
Imports Org.BouncyCastle.Crypto
Imports Org.BouncyCastle.OpenSsl
Imports Org.BouncyCastle.Security
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class ngApiUtilities

    Public Shared Function getHash_SHA256(ByVal _string As String) As Byte()
        '
        ' Kreira SHA256 hash stringa
        '
        Dim _sha256 As SHA256 = SHA256.Create()

        Dim _pPutNalHashBytes As Byte() = Encoding.UTF8.GetBytes(_string)

        Dim _hashBytes2 As Byte()
        Using _sha256
            _hashBytes2 = _sha256.ComputeHash(_pPutNalHashBytes)
        End Using

        Return _hashBytes2

    End Function

    Public Shared Function checkSignature(ByVal _string As String, ByRef _hashBytes2 As Byte(), ByRef _eSignatureBytes As Byte()) As Boolean

        '
        ' Provjera potpisa
        '
        Dim _retVal As Boolean = False
        Dim _rRSA As RSACryptoServiceProvider = ngApiRSAKeys.ImportPublicKey(_string)
        Using _rRSA
            Dim rsaDeformatter = New RSAPKCS1SignatureDeformatter(_rRSA)
            rsaDeformatter.SetHashAlgorithm("SHA256")
            _retVal = rsaDeformatter.VerifySignature(_hashBytes2, _eSignatureBytes)
        End Using

        Return _retVal

    End Function

    Public Shared Function bytesToString(ByRef _bytes As Byte()) As String

        '
        ' Konvertuje byte array u string
        '
        Dim retVal As StringBuilder = New StringBuilder
        Dim HB As Byte
        For Each HB In _bytes
            retVal.Append(String.Format("{0:X2}", HB))
        Next

        Return retVal.ToString()

    End Function

    'Public Shared Function ExportKeyPair(ByVal keyPair As AsymmetricCipherKeyPair) As String

    '    Using memoryStream = New MemoryStream()

    '        Using streamWriter = New StreamWriter(memoryStream)
    '            Dim pemWriter = New PemWriter(streamWriter)
    '            pemWriter.WriteObject(keyPair.[Private])
    '            streamWriter.Flush()
    '            Return Encoding.ASCII.GetString(memoryStream.GetBuffer())
    '        End Using
    '    End Using

    'End Function

    'Public Shared Function ExportPublicKey(ByVal keyPair As AsymmetricCipherKeyPair) As String

    '    Using memoryStream = New MemoryStream()

    '        Using streamWriter = New StreamWriter(memoryStream)
    '            Dim pemWriter = New PemWriter(streamWriter)
    '            pemWriter.WriteObject(keyPair.[Public])
    '            streamWriter.Flush()
    '            Return Encoding.ASCII.GetString(memoryStream.GetBuffer())
    '        End Using
    '    End Using

    'End Function


    Public Shared Function ExportPublicKey(ByRef pCertX509 As X509Certificate2) As String

        Dim publicBuilder As StringBuilder = New StringBuilder()
        publicBuilder.AppendLine("-----BEGIN PUBLIC KEY-----")
        publicBuilder.AppendLine(Convert.ToBase64String(pCertX509.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks))
        publicBuilder.AppendLine("-----END PUBLIC KEY-----")

        Return publicBuilder.ToString()

    End Function

    Private Shared Function BytesToString2(ByVal bytes As Byte()) As String

        Using stream As MemoryStream = New MemoryStream(bytes)

            Using streamReader As StreamReader = New StreamReader(stream)
                Return streamReader.ReadToEnd()
            End Using
        End Using

    End Function

    Public Shared Function addQouteToCsvWords(ByVal pCsvWords As String) As String

        Dim words As String() = pCsvWords.Split(New Char() {","c})

        Dim vals As List(Of String) = New List(Of String)
        For Each word As String In words
            vals.Add("'" + Trim(word) + "'")
        Next

        Dim _retVal As String = String.Join(",", vals)

        Return _retVal

    End Function

    Public Shared Function remItemFromCsvWords(ByVal pCsvWords As String, ByVal pRemItem As String) As String

        Dim words As String() = pCsvWords.Split(New Char() {","c})

        Dim vals As List(Of String) = New List(Of String)
        For Each word As String In words
            If Trim(word) <> Trim(pRemItem) Then
                vals.Add(Trim(word))
            End If
        Next

        Dim _retVal As String = String.Join(",", vals)

        Return _retVal

    End Function

    '
    ' Utilities
    '
    Private Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                row.Add(dc.ColumnName.Trim(), dr(dc))
                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function


End Class
