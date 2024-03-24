Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEvidencijeOdob

    Private dsEmployees As New Data.DataSet
    Private dtEmployees As Data.DataTable

    ''' <summary>
    ''' Lista odobrenja uposlenika - prema pravima koja ima logovani user (pEmployeeID)
    ''' </summary>
    ''' <param name="pEmployeeID"></param>
    ''' <returns></returns>
    Public Function ListOdobrenja(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter

        Dim strSQL As String

        strSQL = <![CDATA[ 
CALL EMP_evid_odob(?pEmployeeID, ?pYear, ?pMonth); 
]]>.Value

        strSQL = strSQL.Replace("?pEmployeeID", pEmployeeID)
        strSQL = strSQL.Replace("?pYear", pYear)
        strSQL = strSQL.Replace("?pMonth", pMonth)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            dsEmployees.Clear()

            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(dsEmployees, "Employees")
            dtEmployees = dsEmployees.Tables("Employees")
        End Using

        Return dtEmployees

    End Function

    Public Sub SetOdobrenja(ByVal pEmployeeID As Integer, ByVal pOdobrio As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)
        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand


        Dim strSQL As String

        strSQL = <![CDATA[ 
CALL EMP_evid_odob(@pEmployeeID, @pOdobrio); 
]]>.Value

        strSQL = strSQL.Replace("?pEmployeeID", pEmployeeID)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)
            mycmd.Parameters.AddWithValue("@pOdobrio", pOdobrio)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' Setovanje odobrenja uposlenika
    ''' </summary>
    ''' <param name="pEmployeeID"></param>
    ''' <param name="pYear"></param>
    ''' <param name="pMonth"></param>
    ''' <param name="pPodnio"></param>
    ''' <param name="pOdobrio"></param>
    ''' <param name="pKontrolisao"></param>
    ''' <param name="pLock"></param>
    Public Sub setOdobrenjaOne(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, _
                            ByVal pPodnio As Integer, ByVal pOdobrio As Integer, ByVal pKontrolisao As Integer, ByVal pLock As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand

        Dim strSQL As String = <![CDATA[ 
REPLACE INTO evd_prisustva_odobrenja (
  `employeeID`, `y`, `m`, `d`, `evd_podnio`, `evd_odobrio`, `evd_kontrolisao`, `evd_locked`
)
SELECT DISTINCT @pEmployeeID, @pYear, @pMonth, 1 AS dSched,  @pPodnio , @pOdobrio, @pKontrolisao, @pLock;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)
            mycmd.Parameters.AddWithValue("@pPodnio", pPodnio)
            mycmd.Parameters.AddWithValue("@pOdobrio", pOdobrio)
            mycmd.Parameters.AddWithValue("@pKontrolisao", pKontrolisao)
            mycmd.Parameters.AddWithValue("@pLock", pLock)
            mycmd.Prepare()

            mycmd.ExecuteNonQuery()

        End Using
    End Sub
    Private Sub GetUserOdobrenjaOne(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, _
                            ByRef pPodnio As Integer, ByRef pOdobrio As Integer, ByRef pKontrolisao As Integer, ByRef pLock As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT DISTINCT eo.evd_podnio, eo.evd_odobrio, eo.evd_kontrolisao, eo.evd_locked
FROM evd_prisustva_odobrenja eo
WHERE eo.employeeID=@pEmployeeId AND eo.m=@pMonth AND  eo.y=@pYear;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeID)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    pPodnio = mydr.Item("evd_podnio")
                    pOdobrio = mydr.Item("evd_odobrio")
                    pKontrolisao = mydr.Item("evd_kontrolisao")
                    pLock = mydr.Item("evd_locked")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using



    End Sub
    Public Sub setOdobPodnioOne(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pPodnio As Integer)

        Dim pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb As Integer
        GetUserOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb)
        If (pOdobrioDb Or pKontrolisaoDb Or pLockedDb) <> 0 Then Exit Sub

        setOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnio, 0, 0, 0)

    End Sub

    Public Sub setOdobOdobrioOne(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pOdobrio As Integer)

        Dim pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb As Integer
        GetUserOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb)
        If (pKontrolisaoDb Or pLockedDb) <> 0 Then Exit Sub

        setOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrio, 0, 0)

    End Sub

    Public Sub setOdobKontrolisaoOne(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pKontrolisao As Integer)

        Dim pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb As Integer
        GetUserOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb)
        If (pLockedDb) <> 0 Then Exit Sub

        setOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrioDb, pKontrolisao, 0)

    End Sub


    Public Sub setOdobLockOne(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, ByVal pLock As Integer)

        Dim pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb As Integer
        GetUserOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLockedDb)

        setOdobrenjaOne(pEmployeeID, pYear, pMonth, pPodnioDb, pOdobrioDb, pKontrolisaoDb, pLock)

    End Sub

    Public Sub setOdobOdobrio(ByVal pLoggedEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, pEmpListaOdob As DataTable)

        For Each el As DataRow In pEmpListaOdob.Rows
            setOdobOdobrioOne(el.Item("EmployeeID"), pYear, pMonth, pLoggedEmployeeID)
        Next

    End Sub

    Public Sub getEmployeeOdob(ByVal pEmployeeId As Integer, ByVal pYear As Integer, ByVal pMonth As Integer, _
                               ByRef pEvdPodnio As String, ByRef pEvdOdobrio As String, ByRef pEvdKontrolisao As String, _
                               ByRef pEvdPodnioInt As Integer, ByRef pEvdOdobrioInt As Integer, ByRef pEvdKontrolisaoInt As Integer, _
                               ByRef pLocked As Integer)

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT eo.employeeID, eo.evd_podnio, eo.evd_odobrio, eo.evd_kontrolisao, eo.evd_locked, e1.LastName AS Pod, e2.LastName AS Odob,  
IFNULL(e3.LastName,'') AS Kont
FROM evd_prisustva_odobrenja eo LEFT JOIN evd_employees e1
ON eo.evd_podnio=e1.EmployeeID
LEFT JOIN  evd_employees e2 ON eo.evd_odobrio=e2.EmployeeID
LEFT JOIN  evd_employees e3 ON eo.evd_kontrolisao=e3.EmployeeID
WHERE eo.employeeID=@pEmployeeId AND eo.y=@pYear AND eo.m=@pMonth;
]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.AddWithValue("@pEmployeeId", pEmployeeId)
            mycmd.Parameters.AddWithValue("@pYear", pYear)
            mycmd.Parameters.AddWithValue("@pMonth", pMonth)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    pEvdPodnio = mydr.Item("Pod")
                    pEvdOdobrio = mydr.Item("Odob")
                    pEvdKontrolisao = mydr.Item("Kont")
                    pEvdPodnioInt = mydr.Item("evd_podnio")
                    pEvdOdobrioInt = mydr.Item("evd_odobrio")
                    pEvdKontrolisaoInt = mydr.Item("evd_kontrolisao")
                    pLocked = mydr.Item("evd_locked")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using

    End Sub
End Class
