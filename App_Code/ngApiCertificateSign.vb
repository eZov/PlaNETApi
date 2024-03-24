Imports Microsoft.VisualBasic
Imports System.Drawing
Imports Syncfusion.Pdf
Imports Syncfusion.Pdf.Graphics
Imports Syncfusion.Pdf.Interactive
Imports Syncfusion.Pdf.Parsing
Imports Syncfusion.Pdf.Security
Imports Syncfusion.EJ2.PdfViewer
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Cryptography
Imports System.Data
Imports Org.BouncyCastle.Crypto
Imports Org.BouncyCastle.Security


'Imports System.Diagnostics

Public Class ngApiCertificateSign

    Dim pos_page As Integer     ' page num: 0,1,2,3...
    Dim pos_scale As Integer

    Dim pos_x As Single
    Dim pos_y As Single
    Dim pos_width As Single
    Dim pos_height As Single

    Public MaxPos As Integer = 0

    Public MarkerPos As String = ""

    '
    ' PDF document signing
    '

    Public Sub Sign(ByVal pInFile As String, ByVal pInImage As String, ByVal pECertificate As String, ByVal pPassword As String, ByVal pSignatureName As String, ByVal pOutFile As String)

        ' load pdf 
        Dim document As PdfLoadedDocument = New PdfLoadedDocument(pInFile)

        If document Is Nothing Then Exit Sub

        'Creates a certificate instance from PFX file with private key
        Dim certificate As PdfCertificate = New PdfCertificate(pECertificate, pPassword)
        Dim signature As PdfSignature = New PdfSignature(document, document.Pages(0), certificate, pSignatureName)  'Creates a digital signature

        signature.Bounds = New System.Drawing.RectangleF(200, 460, 200, 70)

        'Sets an image for signature field
        Dim image As PdfImage = PdfImage.FromFile(pInImage)
        Dim kw As Double = image.Width / 120


        signature.Appearance.Normal.Graphics.DrawImage(image, 0, 0, image.Width / kw, image.Height / kw)

        Dim page As PdfLoadedPage = TryCast(document.Pages(0), PdfLoadedPage)

        'Dim bitmap As PdfBitmap = New PdfBitmap("../../Data/esurkovic_potpis.png")
        'Dim rotation As Integer = -270
        'Dim rect As RectangleF = CalculateTransformation(rotation, bitmap)
        'Dim rectangle As RectangleF = New RectangleF(100, 100, rect.Width, rect.Height)
        'Dim rubberStampAnnotation As PdfRubberStampAnnotation = New PdfRubberStampAnnotation(rectangle)
        'rubberStampAnnotation.Text = "Stamp annotation with 180 degree rotated image"

        'Dim state As PdfGraphicsState = rubberStampAnnotation.Appearance.Normal.Graphics.Save()
        'rubberStampAnnotation.Appearance.Normal.Graphics.TranslateTransform(rect.X, rect.Y)
        'rubberStampAnnotation.Appearance.Normal.Graphics.RotateTransform(rotation)
        'rubberStampAnnotation.Appearance.Normal.Graphics.DrawImage(bitmap, New RectangleF(0, 0, bitmap.Width, bitmap.Height))
        'rubberStampAnnotation.Appearance.Normal.Graphics.Restore(state)

        document.Save(pOutFile)
        document.Close(True)

        'Process.Start("TestFile_stamped.pdf")
    End Sub

    Public Sub Sign(ByVal pInFile As String, ByRef pECertificate As NgECertificate, ByVal pSignatureName As String, ByVal pOutFile As String, Optional ByVal pShiftX As Integer = 200)

        ' load pdf 
        Dim document As PdfLoadedDocument = New PdfLoadedDocument(pInFile)
        If document Is Nothing Then Exit Sub

        'Creates a certificate instance from PFX file with private key
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisPfxFromDb(pECertificate.id)
        Dim pfxStream As MemoryStream = New MemoryStream(cRawData)

        'Dim pfxStream As Stream = New PdfCertificate(pECertificate, pPassword)
        Dim certificate As PdfCertificate = New PdfCertificate(pfxStream, pECertificate.password)

        Dim signature As PdfSignature = New PdfSignature(document, document.Pages(0), certificate, pSignatureName)  'Creates a digital signature

        signature.ContactInfo = ""
        signature.LocationInfo = " "
        signature.Reason = "Digitalni potpis"

        signature.Bounds = New System.Drawing.RectangleF(pShiftX, 460, 150, 35)

        'Sets an image for signature field
        'Dim image As PdfImage = PdfImage.FromFile(pInImage)
        cRawData = _ApiDbFiles.readPotpisImgFromDb(pECertificate.id)
        Dim imgStream As MemoryStream = New MemoryStream(cRawData)

        Dim image As PdfImage = PdfImage.FromStream(imgStream)

        Dim kw As Double = image.Width / 120


        signature.Appearance.Normal.Graphics.DrawImage(image, 0, 0, image.Width / kw, image.Height / kw)

        Dim page As PdfLoadedPage = TryCast(document.Pages(0), PdfLoadedPage)


        document.Save(pOutFile)
        document.Close(True)

    End Sub

    Public Sub Sign(ByVal pInFile As String, ByRef pECertificate As NgECertificate, ByVal pSignatureName As String, ByVal pOutStream As MemoryStream, Optional ByVal pShiftX As Integer = 200)

        ' load pdf 
        Dim document As PdfLoadedDocument = New PdfLoadedDocument(pInFile)

        'Creates a certificate instance from PFX file with private key
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisPfxFromDb(pECertificate.id)
        Dim pfxStream As MemoryStream = New MemoryStream(cRawData)

        'Dim pfxStream As Stream = New PdfCertificate(pECertificate, pPassword)
        Dim certificate As PdfCertificate = New PdfCertificate(pfxStream, pECertificate.password)

        Dim signature As PdfSignature = New PdfSignature(document, document.Pages(0), certificate, pSignatureName)  'Creates a digital signature

        signature.ContactInfo = ""
        signature.LocationInfo = " "
        signature.Reason = "Digitalni potpis"

        signature.Bounds = New System.Drawing.RectangleF(pShiftX, 460, 150, 35)

        'Sets an image for signature field
        'Dim image As PdfImage = PdfImage.FromFile(pInImage)
        cRawData = _ApiDbFiles.readPotpisImgFromDb(pECertificate.id)
        Dim imgStream As MemoryStream = New MemoryStream(cRawData)

        Dim image As PdfImage = PdfImage.FromStream(imgStream)

        Dim kw As Double = image.Width / 120


        signature.Appearance.Normal.Graphics.DrawImage(image, 0, 0, image.Width / kw, image.Height / kw)

        Dim page As PdfLoadedPage = TryCast(document.Pages(0), PdfLoadedPage)


        document.Save(pOutStream)
        document.Close(True)

    End Sub

    Public Function Sign(ByRef pInFileStream As MemoryStream, ByRef pECert As NgECertificate, ByVal pSignatureName As String, ByRef pOutStream As MemoryStream) As Boolean

        ' load pdf 
        Dim document As PdfLoadedDocument = New PdfLoadedDocument(pInFileStream)

        Dim _isCorruptedIn = analyzePdfDocument(pInFileStream).IsCorrupted
        If _isCorruptedIn = True Then Return _isCorruptedIn

        If document Is Nothing Then Return False
        '''''''''
        ' Ok to proceed...

        'Creates a certificate instance from PFX file with private key
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisPfxFromDb(pECert.id)
        Dim pfxStream As MemoryStream = New MemoryStream(cRawData)

        'Dim pfxStream As Stream = New PdfCertificate(pECertificate, pPassword)
        Dim certificate As PdfCertificate = New PdfCertificate(pfxStream, pECert.password)

        'Dim _certificate As System.Security.Cryptography.X509Certificates.X509Certificate2 = New X509Certificate2(cRawData, pECert.password)
        'Dim key As RSACryptoServiceProvider = TryCast(_certificate.PrivateKey, RSACryptoServiceProvider)

        Dim signature As PdfSignature = New PdfSignature(document, document.Pages(pos_page), certificate, pSignatureName)  'Creates a digital signature

        signature.ContactInfo = ""
        signature.LocationInfo = " "
        signature.Reason = "Digitalni potpis"

        signature.Bounds = New System.Drawing.RectangleF(pos_x, pos_y, pos_width, pos_height)

        'Sets an image for signature field
        'Dim image As PdfImage = PdfImage.FromFile(pInImage)
        cRawData = _ApiDbFiles.readPotpisImgFromDb(pECert.id)
        Dim imgStream As MemoryStream = New MemoryStream(cRawData)

        Dim image As PdfImage = PdfImage.FromStream(imgStream)

        Dim kw As Double = image.Width / pos_scale


        signature.Appearance.Normal.Graphics.DrawImage(image, 0, 0, image.Width / kw, image.Height / kw)

        Dim page As PdfLoadedPage = TryCast(document.Pages(0), PdfLoadedPage)

        'document.Save()
        document.Save(pOutStream)
        document.Save()


        'Dim _isCorruptedIn = analyzePdfDocument(pInFileStream).IsCorrupted
        Dim _isCorruptedOut = analyzePdfDocument(pOutStream).IsCorrupted

        document.Close(True)

        Return _isCorruptedOut


    End Function

    Private Shared Function CalculateTransformation(ByVal rotation As Integer, ByVal bitmap As PdfBitmap) As RectangleF
        Dim rectangle As RectangleF = New RectangleF()

        If rotation = -45 Then
            rectangle.X = bitmap.Height
            rectangle.Y = bitmap.Width
            rectangle.Width = bitmap.Width + bitmap.Height
            rectangle.Height = bitmap.Width + bitmap.Height
        ElseIf rotation = -90 Then
            rectangle.X = 0
            rectangle.Y = bitmap.Width
            rectangle.Width = bitmap.Height
            rectangle.Height = bitmap.Width
        ElseIf rotation = -180 Then
            rectangle.X = bitmap.Width
            rectangle.Y = bitmap.Height
            rectangle.Width = bitmap.Width
            rectangle.Height = bitmap.Height
        ElseIf rotation = -270 Then
            rectangle.X = bitmap.Height
            rectangle.Y = 0
            rectangle.Width = bitmap.Height
            rectangle.Height = bitmap.Width
        End If

        Return rectangle
    End Function

    '
    ' Read config data
    '
    Public Sub setSignPosition(ByVal _docNum As Integer, ByVal _potpisNum As Integer)

        Try

            Dim _esignConfig As DataTable = NgApiSys.GetSetting_JsOn("epotpis.putninalog." + _docNum.ToString())

            For i As Integer = 0 To _esignConfig.Rows.Count - 1

                MaxPos = i + 1

                If _esignConfig.Rows(i).Item("id") = _potpisNum Then
                    pos_page = _esignConfig.Rows(i).Item("page")
                    pos_page -= 1

                    pos_scale = _esignConfig.Rows(i).Item("scale")

                    pos_x = _esignConfig.Rows(i).Item("x")
                    pos_y = _esignConfig.Rows(i).Item("y")

                    pos_width = _esignConfig.Rows(i).Item("width")
                    pos_height = _esignConfig.Rows(i).Item("height")

                    MarkerPos = _esignConfig.Rows(i).Item("marker")
                End If
            Next


        Catch ex As Exception

        End Try

    End Sub

    Public Function analyzePdfDocument(ByVal pInFileStream As MemoryStream) As SyntaxAnalyzerResult

        ' load pdf 
        Dim analyzedDocument As PdfDocumentAnalyzer

        Try
            analyzedDocument = New PdfDocumentAnalyzer(pInFileStream)
            Return analyzedDocument.AnalyzeSyntax()
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return Nothing

    End Function

    Public Sub getMarkerPosition(ByVal pInFileStream As MemoryStream)

        ' load pdf 
        Dim loadedDocument As PdfLoadedDocument = New PdfLoadedDocument(pInFileStream)
        If loadedDocument Is Nothing Then Exit Sub





        Dim textSearch As New Dictionary(Of Integer, List(Of RectangleF))()
        loadedDocument.FindText(Me.MarkerPos, textSearch)

        Dim _searchRectangle As Rectangle

        Try
            Me.pos_page = -1
            For Each el In textSearch
                'If el.Key >= Me.pos_page Then
                Me.pos_page += 1
                If el.Value.Count > 0 Then
                    _searchRectangle.X = el.Value(0).X
                    _searchRectangle.Y = el.Value(0).Y
                    Exit For
                End If

            Next

            Me.pos_x = Me.pos_x + _searchRectangle.X
            Me.pos_y = Me.pos_y + _searchRectangle.Y

        Catch ex As Exception
            Dim tst = ex.Message
        End Try

    End Sub


    '
    ' Data signing
    '

    Public Function SignData(ByVal msg As String, ByRef pECert As NgECertificate) As String

        Dim _ApiCert As New ngApiCertificate()
        _ApiCert.getPfxFromDb(pECert.id)
        _ApiCert.setPrivateKey(pECert)

        Dim msgBytes() As Byte = Encoding.UTF8.GetBytes(msg)


        Dim _signer As ISigner = SignerUtilities.GetSigner("SHA256WithRSA")
        _signer.Init(True, _ApiCert.privateKey)
        _signer.BlockUpdate(msgBytes, 0, msgBytes.Length)
        Dim sigBytes() As Byte = _signer.GenerateSignature()

        Dim _signTest As String = Convert.ToBase64String(sigBytes)

        Return _signTest

    End Function

    '
    ' Data signature verification
    '
    Public Function VerifySignature(ByRef pCertX509 As X509Certificate2, ByVal signature As String, ByVal msg As String) As Boolean
        Try
            Dim _dotCert = DotNetUtilities.FromX509Certificate(pCertX509)

            Dim msgBytes As Byte() = Encoding.UTF8.GetBytes(msg)
            Dim sigBytes As Byte() = Convert.FromBase64String(signature)
            Dim signer As ISigner = SignerUtilities.GetSigner("SHA256WithRSA")
            signer.Init(False, _dotCert.GetPublicKey())
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length)
            Return signer.VerifySignature(sigBytes)
        Catch exc As Exception
            Console.WriteLine("Verification failed with the error: " & exc.ToString())
            Return False
        End Try
    End Function

    Public Function VerifySignature2(ByRef pECert As NgECertificate, ByVal pePublicKey As String, ByVal _signedJSON As String, ByVal _stringJSON As String) As Boolean

        Dim _hashBytes2 As Byte() = ngApiUtilities.getHash_SHA256(_stringJSON)
        Dim _eSignatureBytes As Byte() = Convert.FromBase64String(_signedJSON)


        Dim _ApiCertificate As New ngApiCertificate()
        Dim _certX509 = _ApiCertificate.certX509(pECert)         ' Bytes() je već napunjen metodom GenerateCertificate

        Dim _rRSA As RSACryptoServiceProvider = ngApiRSAKeys.ImportPublicKey(pePublicKey)
        Using _rRSA
            Dim rsaDeformatter = New RSAPKCS1SignatureDeformatter(_rRSA)
            rsaDeformatter.SetHashAlgorithm("SHA256")
            Return rsaDeformatter.VerifySignature(_hashBytes2, _eSignatureBytes)
        End Using

        Return False
    End Function


    Public Function getImage(ByVal pECertId As Integer) As MemoryStream
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' IMAGE - preuzmi sliku potpisa iz baze
        '
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisImgFromDb(pECertId)

        Return New MemoryStream(cRawData)


    End Function

    '
    ' Insert signature image to document
    '
    ''' <summary>
    ''' These properties have to be set before sub call: pos_scale, pos_x, pos_y
    ''' </summary>
    ''' <param name="pInFileStream"></param>
    ''' <param name="pImgStream"></param>
    ''' <param name="pOutStream"></param>
    Public Sub insImg2Doc(ByVal pInFileStream As MemoryStream, ByVal pImgStream As MemoryStream, ByVal pOutStream As MemoryStream)

        ' load pdf 
        Dim document As PdfLoadedDocument = New PdfLoadedDocument(pInFileStream)
        If document Is Nothing Then Exit Sub

        Dim image As PdfImage = PdfImage.FromStream(pImgStream)

        Dim kw As Double = image.Width / pos_scale

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' PDF - insert slike na stranicu pdf fajla
        '
        If document.Pages.Count >= pos_page Then
            Dim page As PdfLoadedPage = TryCast(document.Pages(pos_page), PdfLoadedPage)
            Dim graphics As PdfGraphics = page.Graphics
            graphics.DrawImage(image, pos_x, pos_y, image.Width / kw, image.Height / kw)
        End If

        document.Save(pOutStream)
        document.Close(True)

    End Sub




    Public Sub insSignImg2Doc(ByVal pPnDoc As Integer, ByVal pPnSignPos As Integer, ByVal pEcertId As Integer, ByVal pInMemStream As MemoryStream, ByVal pOutMemStream As MemoryStream)

        Dim _imgMemStream As MemoryStream

        ' *** uzimanje POZICIJE POTPISA iz json
        '
        setSignPosition(pPnDoc, pPnSignPos)

        ' Ako su pozicije potpisa definisane markerom preuzmi 
        If MarkerPos <> "" Then
            getMarkerPosition(pInMemStream)
        End If

        _imgMemStream = getImage(pEcertId)
        insImg2Doc(pInMemStream, _imgMemStream, pOutMemStream)

    End Sub


    Public Sub insSignImges2Doc(ByRef pSignPosCertId As Dictionary(Of Integer, Integer), ByVal pPnDoc As Integer, ByRef pInMemStream As MemoryStream, ByRef pOutMemStream As MemoryStream, Optional ByVal pOffset As Integer = 0)


        Dim _signPosCertIdCopy As Dictionary(Of Integer, Integer) = New Dictionary(Of Integer, Integer)(pSignPosCertId)
        pSignPosCertId.Clear()

        For Each el In _signPosCertIdCopy
            pSignPosCertId.Add(el.Key + pOffset, el.Value)
        Next

        For Each el In pSignPosCertId

            insSignImg2Doc(pPnDoc, el.Key, el.Value, pInMemStream, pOutMemStream)

            pInMemStream = pOutMemStream
            pOutMemStream = New MemoryStream()
        Next

        pOutMemStream = pInMemStream
    End Sub



    Public Sub insSignImg(ByVal pInFileStream As MemoryStream, ByRef pECertId As Integer, ByVal pPageNum As Integer, ByVal pOutStream As MemoryStream)

        ' load pdf 
        Dim document As PdfLoadedDocument = New PdfLoadedDocument(pInFileStream)
        If document Is Nothing Then Exit Sub

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' IMAGE - preuzmi sliku potpisa iz baze
        '
        Dim _ApiDbFiles As New ngApiDbFiles
        Dim cRawData() As Byte = _ApiDbFiles.readPotpisImgFromDb(pECertId)
        Dim imgStream As MemoryStream = New MemoryStream(cRawData)

        Dim image As PdfImage = PdfImage.FromStream(imgStream)

        Dim kw As Double = image.Width / pos_scale

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' PDF - insert slike na stranicu pdf fajla
        '
        If document.Pages.Count >= pPageNum Then
            Dim page As PdfLoadedPage = TryCast(document.Pages(pPageNum - 1), PdfLoadedPage)
            Dim graphics As PdfGraphics = page.Graphics
            graphics.DrawImage(image, pos_x, pos_y, image.Width / kw, image.Height / kw)
        End If



        document.Save(pOutStream)
        document.Close(True)

    End Sub

End Class
