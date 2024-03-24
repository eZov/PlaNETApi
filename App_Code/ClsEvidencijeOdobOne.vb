Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsEvidencijeOdobOne

    Private myEmployeeId As Integer
    Private myEvidencijeOdobEmp As ClsEvidencijeOdobEmp

    Public Sub New(ByVal pEmployeeID As Integer, ByVal pYear As Integer, ByVal pMonth As Integer)

        myEmployeeId = pEmployeeID
        myEvidencijeOdobEmp = New ClsEvidencijeOdobEmp
        myEvidencijeOdobEmp.EmployeeId = pEmployeeID
        myEvidencijeOdobEmp.EvdYear = pYear
        myEvidencijeOdobEmp.EvdMonth = pMonth

        getEvidencijeOdobEmp()

    End Sub

    Public Property OdobrenjaEmp As ClsEvidencijeOdobEmp
        Get
            Return myEvidencijeOdobEmp
        End Get
        Set(ByVal value As ClsEvidencijeOdobEmp)
            myEvidencijeOdobEmp = value
        End Set
    End Property

    Private Sub getEvidencijeOdobEmp()

        Dim mycmd As MySql.Data.MySqlClient.MySqlCommand
        Dim mydr As MySql.Data.MySqlClient.MySqlDataReader

        Dim strSQL As String = <![CDATA[ 
SELECT DISTINCT eo.evd_podnio, eo.evd_odobrio, eo.evd_kontrolisao, eo.evd_locked
FROM evd_prisustva_odobrenja eo
WHERE eo.employeeID=@pEmployeeId AND eo.m=@pMonth AND  eo.y=@pYear;
    ]]>.Value

        strSQL = <![CDATA[ 
SELECT eo.employeeID, eo.evd_podnio, eo.evd_odobrio, eo.evd_kontrolisao, eo.evd_locked, 
CONCAT(IFNULL(e1.LastName,''),' ' ,IFNULL(e1.FirstName,''))  AS Pod, CONCAT(IFNULL(e2.LastName,''),' ' ,IFNULL(e2.FirstName,'')) AS Odob,
CONCAT(IFNULL(e3.LastName,''),' ' ,IFNULL(e3.FirstName,'')) AS Kont
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


            mycmd.Parameters.AddWithValue("@pEmployeeId", myEvidencijeOdobEmp.EmployeeId)
            mycmd.Parameters.AddWithValue("@pYear", myEvidencijeOdobEmp.EvdYear)
            mycmd.Parameters.AddWithValue("@pMonth", myEvidencijeOdobEmp.EvdMonth)
            mycmd.Prepare()

            mydr = mycmd.ExecuteReader
            While mydr.Read
                Try
                    myEvidencijeOdobEmp.EvdPodnio = mydr.Item("evd_podnio")
                    myEvidencijeOdobEmp.EvdOdobrio = mydr.Item("evd_odobrio")
                    myEvidencijeOdobEmp.EvdKontrolisao = mydr.Item("evd_kontrolisao")
                    myEvidencijeOdobEmp.EvdLocked = mydr.Item("evd_locked")

                    myEvidencijeOdobEmp.EvdPodnioEmp = mydr.Item("Pod")
                    myEvidencijeOdobEmp.EvdOdobrioEmp = mydr.Item("Odob")
                    myEvidencijeOdobEmp.EvdKontrolisaoEmp = mydr.Item("Kont")
                Catch ex As Exception
                End Try
            End While
            mydr.Close()
        End Using



    End Sub

End Class




Public Class ClsEvidencijeOdobEmp

    Public EmployeeId As Integer
    Public EvdYear As Integer
    Public EvdMonth As Integer

    Public EvdPodnio As Integer
    Public EvdOdobrio As Integer
    Public EvdKontrolisao As Integer
    Public EvdLocked As Integer

    Public EvdPodnioEmp As String
    Public EvdOdobrioEmp As String
    Public EvdKontrolisaoEmp As String

    Public ReadOnly Property Opened As Boolean
        Get
            If EvdPodnio + EvdOdobrio + EvdKontrolisao + EvdLocked = 0 Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property


    Public ReadOnly Property PodnioOnly As Boolean
        Get
            If EvdPodnio > 0 And EvdOdobrio + EvdKontrolisao + EvdLocked = 0 Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property PodnioAndOdobrio As Boolean
        Get
            If EvdPodnio > 0 And EvdOdobrio > 0 And EvdKontrolisao + EvdLocked = 0 Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property Kontrolisao As Boolean
        Get
            If EvdKontrolisao > 0 And EvdLocked = 0 Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property Locked As Boolean
        Get
            If EvdLocked <> 0 Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property
End Class