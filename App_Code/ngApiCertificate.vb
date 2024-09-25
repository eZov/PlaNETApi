Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Org.BouncyCastle.Asn1
Imports Org.BouncyCastle.Asn1.X509
Imports Org.BouncyCastle.Asn1.Utilities
Imports Org.BouncyCastle.Crypto
Imports Org.BouncyCastle.Crypto.Digests
Imports Org.BouncyCastle.Crypto.Generators
Imports Org.BouncyCastle.Crypto.Prng
Imports Org.BouncyCastle.Math
Imports Org.BouncyCastle.Pkcs
Imports Org.BouncyCastle.Pkix
Imports Org.BouncyCastle.Security
Imports Org.BouncyCastle.X509
Imports System.Collections
Imports System.Windows
Imports System.Security.Cryptography.X509Certificates
Imports X509Certificate = Org.BouncyCastle.X509.X509Certificate
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports System.Web.Configuration
Imports System.Security.Cryptography
Imports Org.BouncyCastle.OpenSsl

'Namespace CertificateGenerator.Utility
Public Enum HashType
    SHA1withDSA
    SHA1withECDSA
    SHA224withECDSA
    SHA256withECDSA
    SHA384withECDSA
    SHA512withECDSA
    MD2withRSA
    MD5withRSA
    SHA1withRSA
    SHA224withRSA
    SHA256withRSA
    SHA384withRSA
    SHA512withRSA
    RIPEMD160withRSA
    RIPEMD128withRSA
    RIPEMD256withRSA
End Enum

Public Enum ContainerType
    PKCS7
    PKCS12
    PEM
    DER
End Enum

Public Class ngApiCertificate

    Private certBytes() As Byte
    Private certPathName As String
    Private certFileName As String

    Private certPublicKey As String
    Private certPrivateKey As AsymmetricKeyParameter


    Public Property rawData As Byte()
        Get
            Return certBytes
        End Get
        Set(ByVal value As Byte())
            certBytes = value
        End Set
    End Property

    Public Property publicKey As String
        Get
            Return certPublicKey
        End Get
        Set(ByVal value As String)
            certPublicKey = value
        End Set
    End Property

    Public ReadOnly Property privateKey As AsymmetricKeyParameter
        Get
            Return certPrivateKey
        End Get
    End Property

    ' Da li je spreman red u tabeli evd_potpisi
    '
    Public Function isDbReady(ByRef pECertificate As NgECertificate) As Boolean

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String
        Dim _retValue As Boolean = False

        '
        ' Provjerava ima li red u tabeli i da li je upisana slika potpisa
        '
        strSQL = <![CDATA[
SELECT ep.id, IFNULL(ep.potpis_img_size,0) AS potpis_img_size
FROM evd_potpisi ep
WHERE ep.employee_id = @employee_id AND  ep.aktivan = -1;
    ]]>.Value




        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pECertificate.employeeID)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                Dim _potpis_img_size = -1

                While dbRead.Read()
                    _potpis_img_size = dbRead.GetInt32("potpis_img_size")
                    pECertificate.id = dbRead.GetInt32("id")
                End While

                pECertificate.ecertStatus = IIf(_potpis_img_size > 0, "ready for certificate", "no_image")
                _retValue = IIf(_potpis_img_size > 0, True, False)
            Else
                pECertificate.ecertStatus = "no_signature"
            End If

            dbRead.Close()
        End Using


        Return _retValue

    End Function

    Public Sub getStatus(ByRef pECertificate As NgECertificate)

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String
        Dim _retValue As Boolean = False

        '
        ' Provjerava ima li red u tabeli i da li je upisana slika potpisa
        '
        strSQL = <![CDATA[
SELECT
  ep.id,
  IF(ep.potpis_pfx IS NULL, 0, 1) AS potpis_pfx,    
  IFNULL(ep.potpis_pfx_size, 0) AS potpis_pfx_size,  
  IF(ep.potpis_img IS NULL, 0, 1) AS potpis_img,     
  IFNULL(ep.potpis_img_size, 0) AS potpis_img_size,
  DATE_FORMAT(IFNULL(ep.dat_unosa,'0001-01-01'),'%Y-%m-%dT%H:%i:%s') AS dat_unosa
FROM
  evd_potpisi ep
WHERE ep.employee_id = @employee_id
  AND ep.aktivan = - 1;
    ]]>.Value




        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pECertificate.employeeID)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                Dim _potpis_pfx, _potpis_pfx_size, _potpis_img, _potpis_img_size As Integer
                Dim _potpis_valid_from As String = ""


                While dbRead.Read()
                    _potpis_pfx = dbRead.GetInt32("potpis_pfx")
                    _potpis_pfx_size = dbRead.GetInt32("potpis_pfx_size")
                    _potpis_img = dbRead.GetInt32("potpis_img")
                    _potpis_img_size = dbRead.GetInt32("potpis_img_size")
                    pECertificate.id = dbRead.GetInt32("id")
                    _potpis_valid_from = dbRead.GetString("dat_unosa")
                End While

                pECertificate.ecertStatus = IIf(_potpis_img_size > 0, "ready for certificate", "no_image")
                pECertificate.ecertStatus = IIf(_potpis_pfx_size > 0, "certificate valid", pECertificate.ecertStatus)

                pECertificate.PfxExist = _potpis_pfx
                pECertificate.ImgExist = _potpis_img
                pECertificate.validFrom = _potpis_valid_from
            Else
                pECertificate.ecertStatus = "no_signature"
            End If

            dbRead.Close()
        End Using

    End Sub

    Public Sub getOdobrenje(ByRef pECertificate As NgECertificate)

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String
        Dim _retValue As Boolean = False

        '
        ' Provjerava ima li red u tabeli i da li je upisana slika potpisa
        '
        strSQL = <![CDATA[
SELECT es.`document`, es.`status_from`, es.`status_to`
FROM `evd_potpisi_status` es
WHERE es.`evd_potpisi_id` = @evd_potpisi_id;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@evd_potpisi_id", pECertificate.id)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()

            Dim _result As String = ""
            Dim _document As String = ""

            If dbRead.HasRows Then

                While dbRead.Read()
                    _document = IIf(dbRead.GetInt32("document") = 1, "putni nalog", "")
                    _result = String.Format("Potpisom {0} mijenja status iz: {1} u: {2}", _document.ToUpper(), dbRead.GetString("status_from").ToUpper(), dbRead.GetString("status_to").ToUpper())
                End While

                pECertificate.ecertOdobrenje = _result
            Else
                pECertificate.ecertOdobrenje = ""
            End If

            dbRead.Close()
        End Using

    End Sub

    Public Function getId(ByRef pEmployeeID As Integer) As Integer

        'Dim ConnectionString As String = ApiGlobal.domainConnectionString
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim strSQL As String
        Dim _retValue As Integer = Nothing

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT
  ep.id
FROM
  evd_potpisi ep
WHERE ep.employee_id = @employee_id
  AND ep.aktivan = - 1;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employee_id", pEmployeeID)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                While dbRead.Read()
                    _retValue = dbRead.GetInt32("id")
                End While
            End If

            dbRead.Close()
        End Using


        Return _retValue

    End Function

    Public Sub GenerateCertificate(ByVal pECertificate As NgECertificate)
        Dim extension As String = ".unknown"

        Select Case pECertificate.cType
            Case ContainerType.PKCS12
                extension = ".pfx"
            Case ContainerType.DER
                extension = ".der"
        End Select


        Me.certBytes = Me.Generate(pECertificate)
        Me.certFileName = pECertificate.subjectCN & "-" & pECertificate.employeeID.ToString & "-" & pECertificate.hType.ToString() & extension
        'Me.Save(certName, cert)

    End Sub

    '
    ' Radne funkcije za generisanje certifikata
    '
    Function Generate(ByVal pECertificate As NgECertificate) As Byte()

        Dim kpGenerator = New RsaKeyPairGenerator()
        kpGenerator.Init(New KeyGenerationParameters(New SecureRandom(), pECertificate.bitStrength))

        Dim kp = kpGenerator.GenerateKeyPair()
        Dim cGenerator = New X509V3CertificateGenerator()
        Dim _subjectCN = New X509Name("CN=" & pECertificate.subjectCN)
        Dim _issuerCN = New X509Name("CN=" & pECertificate.issuerCN)
        Dim serial = BigInteger.ProbablePrime(120, New Random())

        cGenerator.SetSerialNumber(serial)
        cGenerator.SetSubjectDN(_subjectCN)
        cGenerator.SetIssuerDN(_issuerCN)

        Dim attrs As IDictionary = New Hashtable()
        attrs(X509Name.C) = pECertificate.X509Name_C
        attrs(X509Name.O) = pECertificate.X509Name_O
        attrs(X509Name.L) = pECertificate.X509Name_L
        attrs(X509Name.ST) = pECertificate.X509Name_ST
        attrs(X509Name.E) = pECertificate.X509Name_E

        Dim ord As IList = New ArrayList(attrs.Keys)

        cGenerator.SetIssuerDN(New X509Name(ord, attrs))
        cGenerator.SetSubjectDN(New X509Name(ord, attrs))
        cGenerator.SetNotBefore(pECertificate.validFrom.AddHours(-4))
        cGenerator.SetNotAfter(pECertificate.validTo)
        cGenerator.SetSignatureAlgorithm(pECertificate.hType.ToString())
        cGenerator.SetPublicKey(kp.[Public])

        ' Key Usege: Digitale Signature or NonRepudiation
        '
        Dim keyUsage = New KeyUsage(X509.KeyUsage.DigitalSignature Or X509.KeyUsage.NonRepudiation)
        cGenerator.AddExtension(X509Extensions.KeyUsage, True, keyUsage)

        Dim publicKeyDer As Byte() = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(kp.[Public]).GetDerEncoded()
        Dim publicKeyDerBase64 As String = Convert.ToBase64String(publicKeyDer)

        'Me.certPublicKey = ngApiUtilities.ExportPublicKey(kp)

        Dim cert = cGenerator.Generate(kp.[Private])

        Return PackageCertificate(cert, pECertificate.cType, kp.[Private], pECertificate.password)    ' byte() certificata PKCS12 

        Return Me.rawData

    End Function

    Private Function PackageCertificate(ByVal cert As X509Certificate, ByVal [cType] As ContainerType, ByVal privateKey As AsymmetricKeyParameter, ByVal password As String) As Byte()
        Dim encoded As Byte() = Nothing

        Select Case [cType]
            Case ContainerType.DER
                encoded = cert.GetEncoded()
            Case ContainerType.PKCS12
                Dim store = New Pkcs12StoreBuilder().Build()
                Dim certEntry As X509CertificateEntry = New X509CertificateEntry(cert)
                store.SetCertificateEntry(cert.SubjectDN.ToString(), certEntry)
                Dim keyEntry As AsymmetricKeyEntry = New AsymmetricKeyEntry(privateKey)
                store.SetKeyEntry(cert.SubjectDN.ToString() & "_key", keyEntry, New X509CertificateEntry() {certEntry})

                Using stream = New MemoryStream()
                    store.Save(stream, password.ToCharArray(), New SecureRandom())
                    stream.Position = 0
                    encoded = New Byte(stream.Length - 1) {}
                    stream.Read(encoded, 0, CInt(stream.Length))
                End Using
        End Select


        Return encoded
    End Function

    Public Function Save(ByVal pDestDir As String) As String

        certPathName = pDestDir + certFileName

        Using outStream As FileStream = New FileStream(certPathName, FileMode.Create, FileAccess.ReadWrite)
            outStream.Write(certBytes, 0, certBytes.Length)
        End Using

        Return certPathName

    End Function

    Public Function VerifyPassword(ByVal pECertificate As NgECertificate) As Boolean

        Dim _id As Integer = getId(pECertificate.employeeID)

        If (_id = Nothing) Then Return False

        Dim _ApiDbFiles As New ngApiDbFiles
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisPfxFromDb(_id)

        ' link:
        ' https://stackoverflow.com/questions/28459042/validate-certificate-password-using-x509certificate2
        '
        Try
            If cRawData IsNot Nothing Then
                Dim certificate = New X509Certificate2(cRawData, pECertificate.password)
            Else
                Return False
            End If

        Catch ex As CryptographicException

            If (ex.HResult And &HFFFF) = &H56 Then
                Return False
            End If

            Throw
        End Try

        Return True
    End Function

    Public Function certX509(ByRef pECert As NgECertificate) As X509Certificate2



        Dim _certX509 As X509Certificate2 = New X509Certificate2()
        Try
            _certX509.Import(Me.certBytes, pECert.password, X509KeyStorageFlags.Exportable Or X509KeyStorageFlags.UserKeySet)

            ' Postavi Public Key
            'Me.publicKey = ngApiUtilities.ExportPublicKey(_certX509)
            Me.publicKey = getPublicKey(_certX509)

        Catch ex As Exception

        End Try

        Return _certX509
    End Function

    Public Function getPfxFromDb(ByVal pId As Integer) As Boolean

        'Creates a certificate instance from PFX file with private key
        Dim _ApiDbFiles As New ngApiDbFiles
        Me.certBytes = _ApiDbFiles.readPotpisPfxFromDb(pId)

        If Me.certBytes.Length > 0 Then Return True

        Return False
    End Function


    Public Function setPrivateKey(ByRef pECert As NgECertificate) As Boolean

        Dim pfxStream As MemoryStream = New MemoryStream(Me.certBytes)


        'Dim _certX509 As X509Certificate2 = New X509Certificate2()
        '_certX509.Import(Me.certBytes, pECert.password, X509KeyStorageFlags.Exportable Or X509KeyStorageFlags.UserKeySet)
        'Console.WriteLine(_certX509.ToString(True))
        '  OVAJ CERTIFIKAT SE NE KORISTI NIGDJE DALJE ???

        Try

            Dim pkcs12 As Pkcs12Store = New Pkcs12Store(pfxStream, pECert.password)
            Dim keyAlias As String = vbNull

            For Each _name As String In pkcs12.Aliases

                If (pkcs12.IsKeyEntry(_name)) Then
                    keyAlias = _name
                End If

            Next

            Dim _key As AsymmetricKeyParameter = pkcs12.GetKey(keyAlias).Key
            Me.certPrivateKey = _key

            'Dim _ce() As X509CertificateEntry = pkcs12.GetCertificateChain(keyAlias)
            'Dim _chain As List(Of Org.BouncyCastle.X509.X509Certificate) = New List(Of Org.BouncyCastle.X509.X509Certificate)(_ce.Length)

            'For Each _c In _ce
            '    _chain.Add(_c.Certificate)
            'Next

        Catch ex As Exception
            Console.Write(ex.ToString)

            Return False
        End Try

        Return False

    End Function

    Public Function getPEM(ByRef pECert As NgECertificate) As String

        Dim pfxStream As MemoryStream = New MemoryStream(Me.certBytes)


        Try

            Dim pkcs12 As Pkcs12Store = New Pkcs12Store(pfxStream, pECert.password)
            Dim keyAlias As String = vbNull

            For Each _name As String In pkcs12.Aliases

                If (pkcs12.IsKeyEntry(_name)) Then
                    keyAlias = _name
                End If

            Next

            '
            ' Konverzija u PEM format
            '
            Dim _cartEntry As X509CertificateEntry = pkcs12.GetCertificate(keyAlias)
            Dim _pemCertX509 As Org.BouncyCastle.X509.X509Certificate = _cartEntry.Certificate
            Dim CertPem As StringBuilder = New StringBuilder()
            Dim CSRPemWriter As PemWriter = New PemWriter(New StringWriter(CertPem))
            CSRPemWriter.WriteObject(_pemCertX509)
            CSRPemWriter.Writer.Flush()
            Dim CertPemText = CertPem.ToString()

            Return CertPemText
            'Console.WriteLine(CertPemText)

        Catch ex As Exception
            Console.Write(ex.ToString)

            Return Nothing
        End Try

        Return False

    End Function

    Public Function getPublicKey(ByRef pCertX509 As X509Certificate2) As String
        Try
            Dim _dotCert = DotNetUtilities.FromX509Certificate(pCertX509)
            '
            ' Konverzija u PEM format
            '
            Dim CertPem As StringBuilder = New StringBuilder()
            Dim CSRPemWriter As PemWriter = New PemWriter(New StringWriter(CertPem))
            CSRPemWriter.WriteObject(_dotCert.GetPublicKey())
            CSRPemWriter.Writer.Flush()
            Dim CertPemText = CertPem.ToString()
            Console.WriteLine(CertPemText)
            Return CertPemText

        Catch exc As Exception
            Console.WriteLine("PEM conversion failed with the error: " & exc.ToString())
            Return Nothing

        End Try

        Return Nothing
    End Function
End Class

'End Namespace


