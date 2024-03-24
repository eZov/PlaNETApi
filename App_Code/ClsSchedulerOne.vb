Imports Microsoft.VisualBasic

Public Class ClsSchedulerOne

    Public pID As Integer
    Public pST As String
    Public pET As String
    Public pOpis As String
    Public pComments As String
    Public pOwnerId As Integer

    Sub New(ByVal pEmployeeID As Integer, ByVal pST As String, ByVal pET As String, ByVal pOpis As String, ByVal pComments As String)

        Me.pOwnerId = pEmployeeID
        Me.pST = pST
        Me.pET = pET
        Me.pOpis = pOpis
        Me.pComments = pComments

    End Sub

    Sub New(ByVal pID As Integer, ByVal pEmployeeID As Integer, ByVal pST As String, ByVal pET As String, ByVal pOpis As String, ByVal pComments As String)

        Me.pID = pID
        Me.pOwnerId = pEmployeeID
        Me.pST = pST
        Me.pET = pET
        Me.pOpis = pOpis
        Me.pComments = pComments

    End Sub
End Class
