Imports Syncfusion.DocIO.DLS
Imports Syncfusion.DocIO
Imports Syncfusion.Windows.Forms
Imports MySql.Data.MySqlClient
Imports System.Data
Imports MySql.Data

Public Class ClsWbSumaEvidencije

    Public cDataAdapter As MySqlDataAdapter
    Public cDataSet As New DataSet
    Private cDbTableName As String
    Private cDbTableNamePrefix As String = "tmp_evd_"

    Public cDataSetLock As New DataSet
    Private cDbTableNameLock As String

    Private cOrgSfr As String = ""

    Private cSumaSatiNula As Boolean = True
    Private cColsRW As New Dictionary(Of Integer, String)

    'Private sfrPlacLista As New Dictionary(Of String, ClsSfrPlacItem)

    Public cSfrPlacLock As String = ""

    Private cAutoload As String = "obracun.autoload"
    Private cSfrPlacLockException As String = ""

    ' Preklapanje GO sa BOL, POD, NPL, PLC
    '
    Public cprklpGoLst As New List(Of Integer)

#Region "Property"


    Public ReadOnly Property DataSet() As DataSet
        Get
            Return cDataSet
        End Get
    End Property

    Public ReadOnly Property TableColsRW() As Dictionary(Of Integer, String)
        Get
            Return cColsRW
        End Get
    End Property

    Public Property TableName() As String
        Get
            Return cDbTableNamePrefix + cDbTableName
        End Get
        Set(ByVal value As String)
            cDbTableName = value
            cOrgSfr = value
        End Set
    End Property

    Public Property TableNameLock() As String
        Get
            Return cDbTableNameLock
        End Get
        Set(ByVal value As String)
            cDbTableName = "tmp_evd_" + value + "_lock"
            cOrgSfr = value
        End Set
    End Property

    Public ReadOnly Property SumaSatiNula() As Boolean
        Get
            Return cSumaSatiNula
        End Get
    End Property

    'Public ReadOnly Property SifranikPlacanja() As Dictionary(Of String, ClsSfrPlacItem)
    '    Get
    '        Return sfrPlacLista
    '    End Get
    'End Property

    Public Property Autoload As String
        Get
            Return cAutoload
        End Get
        Set(ByVal value As String)
            cAutoload = value
        End Set
    End Property

    Public Property LockException As String
        Get
            Return cSfrPlacLockException
        End Get
        Set(ByVal value As String)
            cSfrPlacLockException = value
        End Set
    End Property


#End Region

#Region "____evd_prisustva_suma"

    ''' <summary>
    ''' Load iz tmp_evd_sessionid_yy tabele
    ''' </summary>
    ''' <param name="pMM"></param>
    ''' <param name="pGGGG"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LoadDbEvid(ByVal pMM As Integer, ByVal pGGGG As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataSet
        Dim sqlText As String
        Dim dbResultInt As Integer

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Try
            sqlText = <sql text="CALL `EVDwb_create_tmp_evd_epris`('?DbTableName','?OrgSfr','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)
            sqlText = sqlText.Replace("?OrgSfr", pOrgSfr)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Try
            sqlText = <sql text="CALL `EVDwb_update_tmp_evd_epris`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        'Try
        '    sqlText = <sql text="CALL `EVDwb_autoload2_tmp_evd`('?DbTableName', '', '?MM', '?GGGG', '');"/>.Attribute("text").Value
        '    sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        '    sqlText = sqlText.Replace("?MM", pMM)
        '    sqlText = sqlText.Replace("?GGGG", pGGGG)

        '    ExecuteSQL(sqlText, dbResultInt)

        'Catch ex As Exception
        '    Dim el As New ErrorsAndEvents.ErrorLogger
        '    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        'End Try

        'Try
        '    sqlText = <sql text="CALL `EVDwb_getlock_tmp_evd`('?DbTableName', 'rukovodilac', '?MM', '?GGGG');"/>.Attribute("text").Value
        '    sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        '    sqlText = sqlText.Replace("?MM", pMM)
        '    sqlText = sqlText.Replace("?GGGG", pGGGG)

        '    ExecuteSQL(sqlText, dbResultInt)

        'Catch ex As Exception
        '    Dim el As New ErrorsAndEvents.ErrorLogger
        '    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        'End Try



        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
SELECT  * FROM ?DbTableName ORDER BY Uposlenik;
]]>.Value
                strSQL = strSQL.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
                mycmd.CommandText = strSQL


                cDbTableNameLock = cDbTableNamePrefix + cDbTableName + "_lock"
                'cDataSet.Tables.Add(cDbTableName)

                cDataSet.Tables.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(cDataSet)
                cDataSet.Tables(0).TableName = cDbTableNamePrefix + cDbTableName

            End Using
        Catch ex As Exception

        End Try


        'CheckSumaSati(pMM, pGGGG)

        Return cDataSet

    End Function

    Public Function LoadDbSfrEvidPris() As Dictionary(Of String, String)

        Dim strSQL As String
        Dim mycmd As MySqlCommand
        Dim myrd As MySqlDataReader

        Dim _dictSfrEvid = New Dictionary(Of String, String)

        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                strSQL = <![CDATA[ 
CALL EVDwb_getsifreprissuma();
]]>.Value

                mycmd.CommandText = strSQL

                myrd = mycmd.ExecuteReader

                Do While myrd.Read
                    _dictSfrEvid.Add(myrd.GetString("Sifra"), myrd.GetString("Naziv"))
                Loop
                myrd.Close()


            End Using
        Catch ex As Exception

        End Try

        Return _dictSfrEvid

    End Function

    Public Function LoadDbSfrEvid() As Dictionary(Of String, String)

        Dim strSQL As String
        Dim mycmd As MySqlCommand
        Dim myrd As MySqlDataReader

        Dim _dictSfrEvid = New Dictionary(Of String, String)

        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                strSQL = <![CDATA[ 
CALL EVDwb_getsifre();
]]>.Value

                mycmd.CommandText = strSQL

                myrd = mycmd.ExecuteReader

                Do While myrd.Read
                    _dictSfrEvid.Add(myrd.GetString("Sifra"), myrd.GetString("Naziv"))
                Loop
                myrd.Close()


            End Using
        Catch ex As Exception

        End Try

        Return _dictSfrEvid

    End Function

    Public Sub LoadDbEvid_Lock(ByVal pMM As Integer, ByVal pGGGG As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer

        cDbTableNameLock = cDbTableName + "_lock"
        Try
            sqlText = <sql text="CALL `EVD_create_tmp_evd_lock`('','?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try



        sqlText = <sql text="SELECT  * FROM ?DbTableName ORDER BY Uposlenik;"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableNameLock)

        'cDataSet.Tables.Remove(cDbTableNameLock)

        Try
            cDataSetLock.Tables.Clear()

            Dim myda As MySqlClient.MySqlDataAdapter
            Dim mycmd As MySqlCommand

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = sqlText
                'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)


                'mycmd.Parameters.AddWithValue("@row_number", 0)
                cDataSetLock.Tables.Add(cDbTableNameLock)

                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(cDataSetLock.Tables(cDbTableNameLock))

            End Using


        Catch ex As Exception

        End Try

        'cDataSet.Tables(cDbTableNameLock)


    End Sub

    'Public Sub LoadDbEvid_GoPrkl(ByVal pMM As Integer, ByVal pGGGG As Integer)
    '    Dim sqlText As String

    '    cDbTableNameLock = cDbTableName + "_lock"
    '    Try
    '        sqlText = <sql text="CALL `EVD_create_tmp_evd_goprkl`('','?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
    '        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
    '        sqlText = sqlText.Replace("?MM", pMM)
    '        sqlText = sqlText.Replace("?GGGG", pGGGG)


    '        Dim dbRead As MySqlClient.MySqlDataReader
    '        cprklpGoLst.Clear()

    '        dbRead = ClsDatabaseGeneral.CreateReader(sqlText)
    '        Do While dbRead.Read
    '            cprklpGoLst.Add(ClsDatabaseGeneral.DBGetInteger(dbRead("employeeID")))
    '        Loop
    '        dbRead.Close()
    '        dbRead = Nothing


    '    Catch ex As Exception
    '        Dim el As New ErrorsAndEvents.ErrorLogger
    '        el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
    '    End Try




    'End Sub

    '''' <summary>
    '''' Provjera da li su odobreni redovi tabele evd_prisustva_suma
    '''' Yes - cSumaSatiNula=False, No - cSumaSatiNula=True
    '''' </summary>
    '''' <param name="pMM"></param>
    '''' <param name="pGGGG"></param>
    '''' <remarks></remarks>
    'Public Sub CheckSumaSati(ByVal pMM As Integer, ByVal pGGGG As Integer)
    '    Dim sqlText As String
    '    Dim dbResult As Double



    '    sqlText = <sql text="CALL `EVD_suma_evd_prissuma`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
    '    sqlText = sqlText.Replace("?MM", pMM)
    '    sqlText = sqlText.Replace("?GGGG", pGGGG)

    '    Dim dbRead As MySqlClient.MySqlDataReader

    '    Try
    '        dbRead = ClsDatabaseGeneral.CreateReader(sqlText)
    '        Do While dbRead.Read
    '            dbResult = ClsDatabaseGeneral.DBGetDecimal(dbRead("SumaSati"))
    '        Loop
    '        dbRead.Close()
    '        dbRead = Nothing


    '    Catch ex As Exception

    '    End Try

    '    If dbResult > 0 Then
    '        cSumaSatiNula = False
    '    Else
    '        cSumaSatiNula = True
    '    End If

    'End Sub

    ''' <summary>
    ''' Zakljucuje evidencije - sa definisanim MySql userom (DESKTOP aplikacija)
    ''' </summary>
    ''' <param name="pMM"></param>
    ''' <param name="pGGGG"></param>
    ''' <param name="pUserId"></param>
    Public Sub Zakljuci(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_odobri_evd_prissuma`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    'Public Sub VratiZakljuceno(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer)
    '    Dim sqlText As String
    '    Dim dbResultInt As Integer



    '    sqlText = <sql text="CALL `EVD_odobri_evd_prissuma`('','?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
    '    sqlText = sqlText.Replace("?MM", pMM)
    '    sqlText = sqlText.Replace("?GGGG", pGGGG)
    '    sqlText = sqlText.Replace("?UserId", pUserId)

    '    Try
    '       ExecuteSQL(sqlText, dbResultInt)

    '    Catch ex As Exception
    '        Dim el As New ErrorsAndEvents.ErrorLogger
    '        el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
    '    End Try

    'End Sub

    Public Sub Lock(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer, ByVal pRole As String)
        Dim sqlText As String
        Dim dbResultInt As Integer

        Dim _user As String = "NULL"
        If pUserId > 0 Then _user = pUserId.ToString

        sqlText = <sql text="CALL `EVDwb_lock_evd_prissuma`('?DbTableName', '?Role', '?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM.ToString)
        sqlText = sqlText.Replace("?GGGG", pGGGG.ToString)
        sqlText = sqlText.Replace("?UserId", _user)
        sqlText = sqlText.Replace("?Role", pRole)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub Spasi(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer, ByRef pDictTblRow As Dictionary(Of String, Object))
        Dim sqlText As String
        Dim dbResultInt As Integer


        Dim _sqlText As String = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

        Dim _itemPlac As Double = 0
        Dim _employeeID As Integer = -1
        For Each kvp As KeyValuePair(Of String, Object) In pDictTblRow
            _itemPlac = 0


            Select Case kvp.Key
                Case "EmployeeID"
                    Integer.TryParse(kvp.Value, _employeeID)
                Case "Uposlenik"

                Case Else
                    If _sqlText <> "UPDATE ?DbTableName SET " Then _sqlText += ","
                    Double.TryParse(kvp.Value, _itemPlac)
                    _sqlText += String.Format(" {0} = {1}", kvp.Key, _itemPlac.ToString)

            End Select

        Next

        sqlText = _sqlText + " WHERE EmployeeID = ?EmployeeID"
        sqlText = sqlText.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
        sqlText = sqlText.Replace("?EmployeeID", _employeeID.ToString)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        sqlText = <sql text="CALL `EVDwb_insert_evd_prissuma`('?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        sqlText = <sql text="CALL `EVDwb_update_evd_prissuma_2`('?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub spasiBatch(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer, ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand
        Dim dbResultInt As Integer

        Dim sqlText As String
        Dim _sqlText As String = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

        Dim _itemPlac As Double = 0
        Dim _employeeID As Integer = -1



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
]]>.Value
            mycmd.CommandText = strSQL

            For Each el As Dictionary(Of String, Object) In pRows

                _sqlText = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

                _itemPlac = 0
                _employeeID = -1

                For Each kvp As KeyValuePair(Of String, Object) In el
                    _itemPlac = 0


                    Select Case kvp.Key
                        Case "EmployeeID"
                            Integer.TryParse(kvp.Value, _employeeID)
                        Case "Uposlenik"

                        Case Else
                            If _sqlText <> "UPDATE ?DbTableName SET " Then _sqlText += ","
                            Double.TryParse(kvp.Value, _itemPlac)
                            _sqlText += String.Format(" {0} = '{1}'", kvp.Key, ClsDatabase.DBNum(_itemPlac.ToString))

                    End Select

                Next

                sqlText = _sqlText + " WHERE EmployeeID = ?EmployeeID"
                sqlText = sqlText.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
                sqlText = sqlText.Replace("?EmployeeID", _employeeID.ToString)

                Try
                    ExecuteSQL(sqlText, dbResultInt)
                Catch ex As MySqlException

                End Try

            Next

        End Using


        sqlText = <sql text="CALL `EVDwb_insert_evd_prissuma`('?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        sqlText = <sql text="CALL `EVDwb_update_evd_prissuma_2`('?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub Proracunaj(ByVal pMM As Integer, ByVal pGGGG As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_calculate_tmp_evd`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)


        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub



    Public Sub Preuzmi(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pAutoEnable As String)
        Dim sqlText As String
        Dim dbResultInt As Integer



        sqlText = <sql text="CALL `EVD_autoload2_tmp_evd`('?DbTableName','?MM','?GGGG', '?AutoEnable');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?AutoEnable", pAutoEnable)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub PreuzmiSate(ByVal pMM As Integer, ByVal pGGGG As Integer, Optional ByVal pAutoEnable As String = "")
        Dim sqlText As String
        Dim dbResultInt As Integer


        sqlText = <sql text="CALL `EVD_autoload_sched_tmp_evd`('?DbTableName','?MM','?GGGG', '?AutoEnable');"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cOrgSfr)
        sqlText = sqlText.Replace("?MM", pMM)
        sqlText = sqlText.Replace("?GGGG", pGGGG)
        sqlText = sqlText.Replace("?AutoEnable", pAutoEnable)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

#End Region

#Region "____evd_prisustva_suma WEB aplikacija"


    ''' <summary>
    ''' WEB aplikacija
    ''' </summary>
    ''' <param name="pMM"></param>
    ''' <param name="pGGGG"></param>
    ''' <param name="pOrgSfr"></param>
    Public Sub TransferZakljuci(ByVal pMM As Integer, ByVal pGGGG As Integer, Optional ByVal pOrgSfr As Integer = 1)
        Dim sqlText As String
        Dim dbResultInt As Integer


        Try
            sqlText = <sql text="CALL `EVDwb_transfer_evd_prisustva_suma`('?DbTableName','?OrgSfr','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)
            sqlText = sqlText.Replace("?OrgSfr", pOrgSfr)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

#End Region

#Region "____Xfc"

    ''' <summary>
    ''' WEB aplikacija
    ''' </summary>
    ''' <param name="pMM"></param>
    ''' <param name="pGGGG"></param>
    ''' <param name="pOrgSfr"></param>
    ''' <returns></returns>
    Public Function LoadDbEvidXfc(ByVal pMM As Integer, ByVal pGGGG As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataSet
        Dim sqlText As String
        Dim dbResultInt As Integer

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Try
            sqlText = <sql text="CALL `EVDwb_create_tmp_evd_2`('?DbTableName','?OrgSfr','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)
            sqlText = sqlText.Replace("?OrgSfr", pOrgSfr)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Try
            sqlText = <sql text="CALL `EVDwb_update_tmp_evd_xfc`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try


        Try
            sqlText = <sql text="CALL `EVDwb_getlock_tmp_evd_xfc`('?DbTableName', 'rukovodilac', '?MM', '?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try



        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
SELECT  * FROM ?DbTableName ORDER BY Uposlenik;
]]>.Value
                strSQL = strSQL.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
                mycmd.CommandText = strSQL

                cDbTableNameLock = cDbTableNamePrefix + cDbTableName + "_lock"
                'cDataSet.Tables.Add(cDbTableName)

                cDataSet.Tables.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(cDataSet)
                cDataSet.Tables(0).TableName = cDbTableNamePrefix + cDbTableName

            End Using
        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try


        'CheckSumaSati(pMM, pGGGG)

        Return cDataSet

    End Function

    Public Sub spasiBatchXfc(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pUserId As Integer, ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand
        Dim dbResultInt As Integer

        Dim sqlText As String
        Dim _sqlText As String = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

        Dim _itemPlac As Double = 0
        Dim _employeeID As Integer = -1



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
]]>.Value
            mycmd.CommandText = strSQL

            For Each el As Dictionary(Of String, Object) In pRows

                _sqlText = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

                _itemPlac = 0
                _employeeID = -1


                For Each kvp As KeyValuePair(Of String, Object) In el
                    _itemPlac = 0


                    Select Case kvp.Key
                        Case "EmployeeID"
                            Integer.TryParse(kvp.Value, _employeeID)
                        Case "Uposlenik"

                        Case Else
                            If _sqlText <> "UPDATE ?DbTableName SET " Then _sqlText += ","
                            Double.TryParse(kvp.Value, _itemPlac)

                            _sqlText += String.Format(" {0} = '{1}'", kvp.Key, ClsDatabase.DBNum(_itemPlac.ToString))

                    End Select

                Next

                sqlText = _sqlText + " WHERE EmployeeID = ?EmployeeID"
                sqlText = sqlText.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
                sqlText = sqlText.Replace("?EmployeeID", _employeeID.ToString)

                Try
                    ExecuteSQL(sqlText, dbResultInt)
                Catch ex As MySqlException

                End Try

            Next




            sqlText = <sql text="CALL EVDwb_create_uposleniplacanja();"/>.Attribute("text").Value
            Try
                ExecuteSQL(sqlText, dbResultInt)

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            sqlText = <sql text="CALL `EVDwb_update_xfc_uposleniplacanja_2`('?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)
            sqlText = sqlText.Replace("?UserId", pUserId)

            Try
                ExecuteSQL(sqlText, dbResultInt)

            Catch ex As Exception
                Dim el As New ErrorsAndEvents.ErrorLogger
                el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try

            For Each el As Dictionary(Of String, Object) In pRows

                _employeeID = -1
                Integer.TryParse(el("EmployeeID"), _employeeID)

                '   Ažuriraj sume sati u bazi
                '
                sqlText = <![CDATA[ 
UPDATE `xfc_uposleni-placanja`, 
(  SELECT  IFNULL(SUM(Iznos_Sati),0) AS Suma FROM `xfc_uposleni-placanja`
   WHERE ((Vrsta = CONVERT('RAD' USING cp1250)  OR Vrsta =  CONVERT('ODS' USING cp1250) OR Vrsta =  CONVERT('BOL' USING cp1250)
   OR Vrsta =  CONVERT('' USING cp1250)  OR Vrsta =  CONVERT('' USING cp1250)) AND (Iznos_Sati <> 0) AND
   EmployeeID = ?EmployeeID)  AND (IFNULL(Atribut_2,'') <> CONVERT('ISK' USING cp1250)) GROUP BY EmployeeID) ss
SET Iznos_Sati = ss.Suma        
WHERE Vrsta = 'SUM' AND EmployeeID = ?EmployeeID;
]]>.Value

                sqlText = sqlText.Replace("?EmployeeID", _employeeID.ToString)


                Try
                    ExecuteSQL(sqlText, dbResultInt)

                Catch ex As Exception
                    Dim elex As New ErrorsAndEvents.ErrorLogger
                    elex.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Next

        End Using

    End Sub


    Public Function LoadDbEvidXfcStm(ByVal pMM As Integer, ByVal pGGGG As Integer, Optional ByVal pOrgSfr As Integer = 1) As DataSet
        Dim sqlText As String
        Dim dbResultInt As Integer

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand

        Try
            sqlText = <sql text="CALL `EVDwb_create_tmp_evd_stm`('?DbTableName','?OrgSfr','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)
            sqlText = sqlText.Replace("?OrgSfr", pOrgSfr)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Try
            sqlText = <sql text="CALL `EVDwb_update_tmp_evd_xfc_stm`('?DbTableName','?MM','?GGGG');"/>.Attribute("text").Value
            sqlText = sqlText.Replace("?DbTableName", cDbTableName)
            sqlText = sqlText.Replace("?MM", pMM)
            sqlText = sqlText.Replace("?GGGG", pGGGG)

            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try


        'Try
        '    sqlText = <sql text="CALL `EVDwb_getlock_tmp_evd_xfc`('?DbTableName', 'rukovodilac', '?MM', '?GGGG');"/>.Attribute("text").Value
        '    sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        '    sqlText = sqlText.Replace("?MM", pMM)
        '    sqlText = sqlText.Replace("?GGGG", pGGGG)

        '    ExecuteSQL(sqlText, dbResultInt)

        'Catch ex As Exception
        '    Dim el As New ErrorsAndEvents.ErrorLogger
        '    el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        'End Try



        Try

            Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
SELECT  * FROM ?DbTableName ORDER BY Uposlenik;
]]>.Value
                strSQL = strSQL.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
                mycmd.CommandText = strSQL

                cDbTableNameLock = cDbTableNamePrefix + cDbTableName + "_lock"
                'cDataSet.Tables.Add(cDbTableName)

                cDataSet.Tables.Clear()
                myda = New MySqlClient.MySqlDataAdapter(mycmd)
                myda.Fill(cDataSet)
                cDataSet.Tables(0).TableName = cDbTableNamePrefix + cDbTableName

            End Using
        Catch ex As Exception

        End Try


        'CheckSumaSati(pMM, pGGGG)

        Return cDataSet

    End Function

    Public Sub spasiBatchXfcStm(ByVal pUserId As Integer, ByRef pRows As ArrayList)

        Dim mycmd As MySqlCommand
        Dim dbResultInt As Integer

        Dim sqlText As String
        Dim _sqlText As String = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

        Dim _itemPlac As Double = 0
        Dim _employeeID As Integer = -1



        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
]]>.Value
            mycmd.CommandText = strSQL


            For Each el As Dictionary(Of String, Object) In pRows

                _sqlText = <![CDATA[UPDATE ?DbTableName SET ]]>.Value

                _itemPlac = 0
                _employeeID = -1

                For Each kvp As KeyValuePair(Of String, Object) In el
                    _itemPlac = 0


                    Select Case kvp.Key
                        Case "EmployeeID"
                            Integer.TryParse(kvp.Value, _employeeID)
                        Case "Uposlenik"

                        Case Else
                            If _sqlText <> "UPDATE ?DbTableName SET " Then _sqlText += ","
                            Double.TryParse(kvp.Value, _itemPlac)
                            _sqlText += String.Format(" {0} = '{1}'", kvp.Key, ClsDatabase.DBNum(_itemPlac.ToString))

                    End Select

                Next

                sqlText = _sqlText + " WHERE EmployeeID = ?EmployeeID"
                sqlText = sqlText.Replace("?DbTableName", cDbTableNamePrefix + cDbTableName)
                sqlText = sqlText.Replace("?EmployeeID", _employeeID.ToString)

                Try
                    ExecuteSQL(sqlText, dbResultInt)
                Catch ex As MySqlException

                End Try

            Next

        End Using


        sqlText = <sql text="CALL EVDwb_create_uposleniplacanja();"/>.Attribute("text").Value

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        sqlText = <sql text="CALL `EVDwb_update_xfc_uposleniplacanja_stm`('?DbTableName','?MM','?GGGG', '?UserId' );"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?UserId", pUserId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

#End Region



#Region "____EvdPris"

    Public Sub LockEvdPris(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pEmployeeId As Integer, ByVal pRole As String, ByVal pCurrEmpId As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer


        sqlText = <sql text="CALL EVDwb_lock_evd_pris('?DbTableName','?Role',?EmployeeId,'?MM','?GGGG',?CurrEmpId);"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM.ToString)
        sqlText = sqlText.Replace("?GGGG", pGGGG.ToString)
        sqlText = sqlText.Replace("?EmployeeId", pEmployeeId)
        sqlText = sqlText.Replace("?Role", pRole)
        sqlText = sqlText.Replace("?CurrEmpId", pCurrEmpId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Sub UnlockEvdPris(ByVal pMM As Integer, ByVal pGGGG As Integer, ByVal pEmployeeId As Integer, ByVal pRole As String, ByVal pCurrEmpId As Integer)
        Dim sqlText As String
        Dim dbResultInt As Integer


        sqlText = <sql text="CALL EVDwb_unlock_evd_pris('?DbTableName','?Role',?EmployeeId,'?MM','?GGGG',?CurrEmpId);"/>.Attribute("text").Value
        sqlText = sqlText.Replace("?DbTableName", cDbTableName)
        sqlText = sqlText.Replace("?MM", pMM.ToString)
        sqlText = sqlText.Replace("?GGGG", pGGGG.ToString)
        sqlText = sqlText.Replace("?EmployeeId", pEmployeeId)
        sqlText = sqlText.Replace("?Role", pRole)
        sqlText = sqlText.Replace("?CurrEmpId", pCurrEmpId)

        Try
            ExecuteSQL(sqlText, dbResultInt)

        Catch ex As Exception
            Dim el As New ErrorsAndEvents.ErrorLogger
            el.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub
#End Region


    Private Sub ExecuteSQL(ByVal pStrSql As String, ByRef pResult As Integer)

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 

]]>.Value
            strSQL = pStrSql
            mycmd.CommandText = strSQL


            mycmd.ExecuteNonQuery()

        End Using

    End Sub
End Class

'Public Class ClsSfrPlacItem

'    Public UniqueId As Integer

'    Public Sifra As String
'    Public Naziv As String
'    Public Vrsta As String

'    Public Overrides Function ToString() As String
'        Return Naziv + " - " + Sifra.Substring(2, Sifra.Length - 2)
'    End Function
'End Class


