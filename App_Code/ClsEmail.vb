Imports System.Data
Imports System.Net
Imports System.Net.Mail
Imports MySql.Data
Imports System.Collections.Generic

Public Class ClsEmail

    Public smtpMail As String = ""
    Public smtpPassword As String = ""
    Public smtpURL As String = ""
    Public smtpPort As String = ""
    Public smtpSSL As Boolean = False

    Public cInFolder As String
    Public cOutFolder As String


    Public Sub New()

        Dim _emId As Integer


        Try
            ' uzmi id iz tabele smtp postavki - za specifičnu postavku definisanu parametrom obracun.emailSmtp (tabela. sys_configuration)
            ''
            If Not Integer.TryParse(ClsConfigSettings.GetSetting("obracun.emailSmtp"), _emId) Then
                _emId = -1
            End If

            Dim _emSmtp As DataTable = ClsConfigSettings.GetSetting_JsOn("obracun.emailSmtp")

            For i As Integer = 0 To _emSmtp.Rows.Count - 1
                If _emSmtp.Rows(i).Item("id") = _emId Then
                    smtpURL = _emSmtp.Rows(i).Item("host")
                    smtpPort = _emSmtp.Rows(i).Item("port")
                    smtpSSL = If(_emSmtp.Rows(i).Item("SSL") = "true", True, False)

                    smtpMail = _emSmtp.Rows(i).Item("username")
                    smtpPassword = _emSmtp.Rows(i).Item("password")
                End If
            Next


        Catch ex As Exception

        End Try

    End Sub



    Public Function SendEmailAsTest(ByVal sMailFrom As String, ByVal sMailTo As String, ByVal fileAtt As String, Optional ByVal sMailSSL As Boolean = False) As Boolean

        Dim mailFrom As MailAddress = New MailAddress(sMailFrom)
        Dim mailTo As MailAddress = New MailAddress(sMailTo)
        Dim NewMsg As MailMessage = New MailMessage(mailFrom, mailTo)

        Dim retStatus As Boolean = False

        NewMsg.Subject = "Test email SMTP server"
        NewMsg.Body = "Ako je poruka primljena, znači da SMTP server prihvata slanje emailova sa ovim postavkama"

        'For File Attachment, more files can also be attached
        'Dim Att As Attachment = New Attachment(fileAtt)
        'NewMsg.Attachments.Add(Att)

        Dim Smtp As SmtpClient = New SmtpClient(smtpURL, smtpPort)
        Smtp.UseDefaultCredentials = False
        Smtp.Credentials = New NetworkCredential(smtpMail, smtpPassword)
        Smtp.DeliveryMethod = SmtpDeliveryMethod.Network
        Smtp.EnableSsl = sMailSSL

        Try
            Smtp.Send(NewMsg)
            retStatus = True
        Catch ex As Exception

            MsgBox(ex.ToString)


        End Try

        Return retStatus

    End Function

    Public Function SendAsyncEmailAsTest(ByVal sMailFrom As String, ByVal sMailTo As String, ByVal fileAtt As String, Optional ByVal sMailSSL As Boolean = False) As Boolean

        Dim mailFrom As MailAddress = New MailAddress(sMailFrom)
        Dim mailTo As MailAddress = New MailAddress(sMailTo)
        Dim NewMsg As MailMessage = New MailMessage(mailFrom, mailTo)

        Dim retStatus As Boolean = False

        NewMsg.Subject = "Test email SMTP server"
        NewMsg.Body = "Ako je poruka primljena, znači da SMTP server prihvata slanje emailova sa ovim postavkama"

        'For File Attachment, more files can also be attached
        'Dim Att As Attachment = New Attachment(fileAtt)
        'NewMsg.Attachments.Add(Att)

        Dim Smtp As SmtpClient = New SmtpClient(smtpURL, smtpPort)
        Dim state As [Object] = NewMsg

        Smtp.UseDefaultCredentials = False
        Smtp.Credentials = New NetworkCredential(smtpMail, smtpPassword)
        Smtp.DeliveryMethod = SmtpDeliveryMethod.Network
        Smtp.EnableSsl = sMailSSL

        Try
            Smtp.SendAsync(NewMsg, state)
            retStatus = True
        Catch ex As Exception

            MsgBox(ex.ToString)


        End Try

        Return retStatus

    End Function

    Public Function SendEmail(ByVal sMailFrom As String, ByVal sMailTo As String, ByVal sFileAtt As String,
                                       ByVal sSubject As String, ByVal sBody As String,
                                       Optional ByVal sMailSSL As Boolean = False) As String

        Dim MailMsg As MailMessage = New MailMessage()
        Dim mailFrom As MailAddress = New MailAddress(sMailFrom)
        Dim mailTo As MailAddress

        Dim fReturn As String = "OK"

        MailMsg.From = mailFrom

        If sMailTo.Contains(",") Then
            Dim _emailTo As String() = sMailTo.Split(",")
            For Each el In _emailTo
                If el.Contains("@") Then
                    mailTo = New MailAddress(el)
                    MailMsg.To.Add(mailTo)
                End If
            Next
        Else
            If sMailTo.Contains("@") Then
                mailTo = New MailAddress(sMailTo)
                MailMsg.To.Add(mailTo)
            End If
        End If

        If MailMsg.To.Count = 0 Then
            fReturn = "False"
            Return fReturn
        End If


        'sBody = My.Resources.emailHtmlBody
        MailMsg.Subject = sSubject
        MailMsg.Body = sBody
        MailMsg.IsBodyHtml = True

        'For File Attachment, more files can also be attached
        If sFileAtt <> "" Then
            Dim Att As Attachment = New Attachment(sFileAtt)
            MailMsg.Attachments.Add(Att)
        End If


        Dim Smtp As SmtpClient = New SmtpClient(smtpURL, smtpPort)
        Smtp.UseDefaultCredentials = False
        Smtp.Credentials = New NetworkCredential(smtpMail, smtpPassword)
        Smtp.DeliveryMethod = SmtpDeliveryMethod.Network
        Smtp.EnableSsl = sMailSSL


        Try
            Smtp.Send(MailMsg)

        Catch ex As Exception
            'MsgBox(ex.ToString)
            fReturn = ex.Message
        End Try



        Return fReturn

    End Function

    Public Sub getEmail(ByRef pEmails As Dictionary(Of Integer, String))

        Dim lstEmpID As New List(Of Integer)

        For Each el As KeyValuePair(Of Integer, String) In pEmails
            lstEmpID.Add(el.Key)
        Next

        Dim strSQL As String

        strSQL = <sql text="
SELECT el.Email_pos FROM `evd_employees_lpd` el WHERE el.`EmployeeID` = @pEmployeeId;
"/>.Attribute("text").Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            For Each el As Integer In lstEmpID

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@pEmployeeId", el)
                mycmd.Prepare()

                dbRead = mycmd.ExecuteReader
                Do While dbRead.Read
                    pEmails.Item(el) = dbRead.GetString("Email_pos").ToString
                Loop
                dbRead.Close()

            Next


        End Using

    End Sub
End Class

