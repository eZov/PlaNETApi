Imports Microsoft.VisualBasic
Imports MySql.Data
Imports System
Imports System.Configuration
Imports MySql.Data.Types
Imports System.Globalization

Public Class ClsDatabase
    'Private cnstr As String = ConfigurationManager.ConnectionStrings("Evidencije").ConnectionString
    Private cnstr As String = ApiGlobal.domainConnectionString

    Private sql As String
    Private dbConnection As MySqlClient.MySqlConnection
    Private dbCmd As MySqlClient.MySqlCommand

    Public Shared HoldTransaction As MySql.Data.MySqlClient.MySqlTransaction

    Public Sub New()
        connOpen()
    End Sub

    Public ReadOnly Property Connection() As MySqlClient.MySqlConnection
        Get
            Return (dbConnection)
        End Get
    End Property

    Public Sub sqlExecute(ByVal sql As String)
        Dim conn As New MySqlClient.MySqlConnection(cnstr)
        conn.Open()
        Try
            Dim cmd As New MySqlClient.MySqlCommand(sql, conn)
            cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
        conn.Close()
    End Sub

    Public Sub connOpen()
        dbConnection = New MySqlClient.MySqlConnection(cnstr)
        dbConnection.Open()
    End Sub

    Public Sub connClose()
        dbConnection.Close()
        dbConnection.Dispose()
    End Sub

    Public Sub connCreateCommand()
        dbCmd = dbConnection.CreateCommand
    End Sub

    Public Sub sqlExecute_LeaveOpenConn(ByVal sql As String)
        Try
            dbCmd.CommandText = sql
            dbCmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Function Date_MySQLFormat(ByVal webDate As DateTime) As String
        Dim format As String = "yyyy/MM/dd"
        Dim retVal As String

        Try
            retVal = webDate.ToString(format)

        Catch ex As Exception
            retVal = ""
        End Try

        Return retVal
    End Function

    Public Function CreateReader(ByVal sqlText As String) As MySqlClient.MySqlDataReader
        ' ----- Given a SQL statement, return a data reader.
        Dim dbCommand As MySqlClient.MySqlCommand
        Dim dbReader As MySqlClient.MySqlDataReader

        ' ----- Try to run the statement. Note that no error trapping is done
        '       here. It is up to the calling routine to set up error checking.

        dbCommand = New MySqlClient.MySqlCommand(sqlText, dbConnection)


        If Not (HoldTransaction Is Nothing) Then dbCommand.Transaction = HoldTransaction
        dbReader = dbCommand.ExecuteReader()
        dbCommand = Nothing
        Return dbReader
    End Function

#Region "__Transations"


    Public Sub TransactionBegin()
        ' ----- Create a new database transaction.
        On Error Resume Next
        HoldTransaction = dbConnection.BeginTransaction()
    End Sub

    Public Sub TransactionCommit()
        ' ----- Ignore if there is no transaction.
        On Error Resume Next
        If (HoldTransaction Is Nothing) Then Return

        ' ----- Commit the transaction.
        HoldTransaction.Commit()
        HoldTransaction = Nothing
    End Sub

    Public Sub TransactionRollback()
        ' ----- Ignore if there is no transaction.
        On Error Resume Next
        If (HoldTransaction Is Nothing) Then Return

        ' ----- Rollback the transaction.
        HoldTransaction.Rollback()
        HoldTransaction = Nothing
    End Sub

#End Region


#Region "__Utilities"

    Public Shared Function DBDate(ByVal origText As String) As String
        If (Trim(origText) = "") Then
            Return "NULL"
        ElseIf (IsDate(origText)) Then
            Dim _dt As Date = CDate(origText)
            _dt.ToString("yyyy.MM.dd")
            Return "'" & _dt & "'"
        Else
            Return "NULL"
        End If
    End Function

    Public Shared Function DBDate(ByVal origDate As Date) As String

        Return "'" & origDate.ToString("yyyy.MM.dd") & "'"

    End Function

    Public Shared Function DBTime(ByVal origDate As Date) As String

        Return "'" & origDate.ToString("HH:mm:ss") & "'"
        'Return "'" & Format(origDate, "HH:mm:ss") & "'"
    End Function


    Public Shared Function DBTime(ByVal origDate As TimeSpan) As String

        Dim time As DateTime = DateTime.Today.Add(origDate)
        Return "'" & time.ToString("HH:mm:ss") & "'"
        'Return "'" & Format(time, "HH:mm:ss") & "'"

    End Function

    Public Shared Function DBFontStyle(ByVal useFont As System.Drawing.Font) As String
        ' ----- Given a font, extract to a string the possible letters "BIUK"
        '      (bold, italic, underline, strikeout). See the BuildFontStyle
        '      function for the reverse of this function.
        Dim resultStyle As String

        ' ----- Process the string.
        resultStyle = ""
        If (useFont.Bold = True) Then resultStyle &= "B"
        If (useFont.Italic = True) Then resultStyle &= "I"
        If (useFont.Underline = True) Then resultStyle &= "U"
        If (useFont.Strikeout = True) Then resultStyle &= "K"
        Return resultStyle
    End Function

    Public Shared Function DBGetDate(ByRef dataField As Object) As Date
        ' ----- Return the decimal equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return New Date(1900, 1, 1)
        Else
            Dim _Date As Date = New Date(1900, 1, 1)
            Dim _dataField As MySqlDateTime = CType(dataField, Types.MySqlDateTime)

            Try
                _Date = New Date(_dataField.Year, _dataField.Month, _dataField.Day)

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
            Return _Date
        End If
    End Function

    Public Shared Function DBGetDecimal(ByRef dataField As Object) As Decimal
        ' ----- Return the decimal equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return 0@
        Else
            Return CDec(dataField)
        End If
    End Function

    Public Shared Function DBGetInteger(ByRef dataField As Object) As Integer
        ' ----- Return the integer equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return 0
        Else
            Return CInt(dataField)
        End If
    End Function

    Public Shared Function DBGetText(ByRef dataField As Object) As String
        ' ----- Return the text equivalent of an optional database field.
        If (IsDBNull(dataField) = True) Then
            Return ""
        Else
            Return CStr(dataField)
        End If
    End Function

    Public Shared Function DBGetBoolean(ByVal dbBoolean As Integer) As Boolean
        ' ----- Return the text equivalent of an optional database field.
        If (IsDBNull(dbBoolean) = True) Then
            Return False
        ElseIf dbBoolean <> 0 Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Shared Function DBGetBoolean2(ByVal dataField As Object) As Boolean
        ' ----- Return the text equivalent of an optional database field.

        Dim dbBoolean As Boolean = False

        Try
            dbBoolean = BitConverter.ToBoolean(dataField, 0)
        Catch ex As Exception

        End Try

        Return dbBoolean
    End Function

    Public Shared Function DBBoolean(ByVal origBoolean As Boolean) As Integer
        ' ----- Return the text equivalent of an optional database field.
        If (IsDBNull(origBoolean) = True) Then
            Return 0
        ElseIf origBoolean = False Then
            Return 0
        Else
            Return -1
        End If
    End Function
    Public Shared Function DBNum(ByVal origText As String) As String
        ' ----- Prepare a number for insertion in a SQL statement.
        If (Trim(origText) = "") Then
            Return "NULL"
        Else
            Return Replace(origText, ",", ".").Trim(" ")
        End If
    End Function

    Public Shared Function DBDecimal(ByVal origText As String) As Decimal
        Dim _dbDec As Decimal = 0

        If (Trim(origText) = "") Then
            Return _dbDec
        Else
            'Dim _dbDecTxt As String = Replace(origText, ",", ".").Trim(" ")
            If Regex.IsMatch(origText, "^\d*[0-9](|,\d*[0-9])?$") Then
                ' no cCulture koristi , za decimalni broj
                _dbDec = Convert.ToDecimal(origText, System.Globalization.CultureInfo.GetCultureInfo("no"))
            Else
                _dbDec = 0
            End If

        End If

        Return _dbDec
    End Function

    Public Shared Function DBText(ByVal origText As String) As String
        ' ----- Prepare a string for insertion in a SQL statement.
        If (Trim(origText) = "") Then
            'Return "NULL"
            Return "'" & Replace(origText, "'", "''") & "'"
        Else
            Return "'" & Replace(origText, "'", "''") & "'"
        End If
    End Function

    Public Shared Function DigitsOnly(ByVal origText As String) As String
        ' ----- Return only the digits found in a string.
        Dim destText As String
        Dim counter As Integer

        ' ----- Examine each character.
        destText = ""
        For counter = 1 To Len(origText)
            If (IsNumeric(Mid(origText, counter, 1))) Then _
                destText &= Mid(origText, counter, 1)
        Next counter
        Return destText
    End Function
#End Region

End Class
