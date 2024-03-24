Imports System.IO
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ngApiDbFiles
    Private sqlText As String

    Private cRawData() As Byte
    Private cRawSize As UInt32

    Private fs As FileStream

    Public Function writTestFILE2Db(ByVal pId As Integer, ByRef pRawData() As Byte, ByVal pRawSize As UInt32) As Integer

        Dim _SQL As String = <![CDATA[ 
UPDATE
  evd_potpisi
SET
  potpis_pfx = @raw_data,
  potpis_pfx_size = @raw_size
WHERE id = @id;
]]>.Value

        cRawData = pRawData
        cRawSize = pRawSize

        Dim myResult As Integer = writeToDb(pId, _SQL)

        Return myResult
    End Function


    Public Function writePotpisPfx2Db(ByVal pId As Integer, ByRef pRawData() As Byte, ByVal pPublicKey As String) As Integer

        Dim _SQL As String = <![CDATA[ 
UPDATE
  evd_potpisi
SET
  potpis_pfx = @raw_data,
  potpis_pfx_size = @raw_size,
  potpis_publickey = @potpis_publickey,
  dat_unosa = NOW()
WHERE id = @id;
]]>.Value

        Me.cRawData = pRawData
        Me.cRawSize = pRawData.Count()
        'Dim myResult As Integer = writeToDb(pId, _SQL)

        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@raw_data", cRawData)
                mycmd.Parameters.AddWithValue("@raw_size", cRawSize)
                mycmd.Parameters.AddWithValue("@potpis_publickey", pPublicKey)
                mycmd.Parameters.AddWithValue("@id", pId)
                mycmd.Prepare()

                myResult = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using


        Return myResult
    End Function

    Public Function writePotpisPfx2Db(ByVal pId As Integer, ByVal pFilePath As String) As Integer

        Dim _SQL As String = <![CDATA[ 
UPDATE
  evd_potpisi
SET
  potpis_pfx = @raw_data,
  potpis_pfx_size = @raw_size
WHERE id = @id;
]]>.Value

        getRawData(pFilePath)
        Dim myResult As Integer = writeToDb(pId, _SQL)

        Return myResult
    End Function


    Public Function writePotpisImg2Db(ByVal pId As Integer, ByRef pRawData() As Byte) As Integer

        Dim _SQL As String = <![CDATA[ 
UPDATE
  evd_potpisi
SET
  potpis_img = @raw_data,
  potpis_img_size = @raw_size
WHERE id = @id;
]]>.Value

        Me.cRawData = pRawData
        Me.cRawSize = pRawData.Count()
        Dim myResult As Integer = writeToDb(pId, _SQL)

        Return myResult
    End Function

    Public Function writePotpisImg2Db(ByVal pId As Integer, ByVal pFilePath As String) As Integer

        Dim _SQL As String = <![CDATA[ 
UPDATE
  evd_potpisi
SET
  potpis_img = @raw_data,
  potpis_img_size = @raw_size
WHERE id = @id;
]]>.Value

        getRawData(pFilePath)
        Dim myResult As Integer = writeToDb(pId, _SQL)

        Return myResult

    End Function


    Public Function removePotpisImg2Db(ByVal pId As Integer) As Integer

        Dim _SQL As String = <![CDATA[ 
UPDATE
  evd_potpisi
SET
  potpis_img = NULL,
  potpis_img_size = NULL
WHERE id = @id
AND potpis_pfx IS NULL
AND potpis_pfx_size IS NULL;
]]>.Value


        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@id", pId)
                mycmd.Prepare()

                myResult = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using

        Return myResult

    End Function




    ''' <summary>
    ''' Upisuje u db file pdf putninalog sa potpisom. Return -1, -2 ako nije uspješno
    ''' </summary>
    ''' <param name="pPnId"></param>
    ''' <param name="pPnDoc"></param>
    ''' <param name="pPotpisId"></param>
    ''' <param name="pFilename"></param>
    ''' <returns></returns>
    Public Function writePdf2Db(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pPotpisId As Integer, ByVal pFilename As String) As Integer

        Dim _SQL As String = <![CDATA[ 
INSERT INTO `putninalog_doc_documents` (
  `pn_idByVal pPnId As Integer,
  `pn_doc`,
  `evd_potpisi_id`,
  `filename`,
  `filepath`
)
VALUES
  (
    @pn_id,
    @pn_doc,
    @evd_potpisi_id,
    @filename,
    @filepath
  );
]]>.Value

        Dim mycmd As MySqlCommand
        Dim myLastInsID As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@pn_id", pPnId)
                mycmd.Parameters.AddWithValue("@pn_doc", pPnDoc)
                mycmd.Parameters.AddWithValue("@evd_potpisi_id", pPotpisId)
                mycmd.Parameters.AddWithValue("@filename", pFilename)
                mycmd.Parameters.AddWithValue("@filepath", "db")
                mycmd.Prepare()

                myLastInsID = mycmd.ExecuteScalar()
                myLastInsID = mycmd.LastInsertedId
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Dim myResult As Integer = -1

        _SQL = <![CDATA[ 
UPDATE
  `putninalog_doc_documents`
SET
  `filesize` = @raw_size,
  `filebody` = @raw_data
WHERE `id` = @id;
]]>.Value

        If myLastInsID > 0 Then
            ' getRawData(pFilePath)
            myResult = writeToDb(myLastInsID, _SQL)
        End If

        Return myResult

    End Function

    Public Function writePdf2File(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer, ByVal pPotpisId As Integer, ByVal pFilename As String, ByVal pFilepath As String) As Integer

        Dim _SQL As String = <![CDATA[ 
INSERT INTO `putninalog_doc_documents` (
  `pn_id`,
  `pn_doc`,
  `pn_potpis`,
  `evd_potpisi_id`,
  `filename`,
  `filepath`
)
VALUES
  (
    @pn_id,
    @pn_doc,
    @pn_potpis,
    @evd_potpisi_id,
    @filename,
    @filepath
  );
]]>.Value

        Dim mycmd As MySqlCommand
        Dim myLastInsID As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@pn_id", pPnId)
                mycmd.Parameters.AddWithValue("@pn_doc", pPnDoc)
                mycmd.Parameters.AddWithValue("@pn_potpis", pSignPos)
                mycmd.Parameters.AddWithValue("@evd_potpisi_id", pPotpisId)
                mycmd.Parameters.AddWithValue("@filename", pFilename)
                mycmd.Parameters.AddWithValue("@filepath", pFilepath)
                mycmd.Prepare()

                myLastInsID = mycmd.ExecuteScalar()
                myLastInsID = mycmd.LastInsertedId
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End Using

        Dim myResult As Integer = -1


        pFilepath = HttpContext.Current.Server.MapPath(pFilepath)

        If myLastInsID > 0 Then
            myResult = writeToFile(pFilename, pFilepath)
        End If

        Return myResult

    End Function

    Public Sub getRawData(ByVal pFilePath As String)

        fs = New FileStream(pFilePath, FileMode.Open, FileAccess.Read)
        cRawSize = fs.Length

        cRawData = New Byte(cRawSize) {}
        fs.Read(cRawData, 0, cRawSize)
        fs.Close()

    End Sub

    Public Sub setRawDataFromStream(ByVal pMemStream As MemoryStream)

        'cRawSize = pMemStream.Length
        'cRawData = New Byte(cRawSize) {}

        'pMemStream.Read(cRawData, 0, cRawSize + 1)

        cRawData = pMemStream.ToArray()
        cRawSize = cRawData.Length

    End Sub

    Public Function writeToDb(ByVal pId As Integer, ByVal pSql As String) As Integer

        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = pSql
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@raw_data", cRawData)
                mycmd.Parameters.AddWithValue("@raw_size", cRawSize)
                mycmd.Parameters.AddWithValue("@id", pId)
                mycmd.Prepare()

                myResult = mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using

        Return myResult

    End Function

    Public Function writeToFile(ByVal pFilename As String, ByVal pFilepath As String) As Boolean

        ' url: http://net-informations.com/q/faq/memory.html

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                Dim ms As New MemoryStream(cRawData)
                Dim file As New FileStream(pFilepath + pFilename, FileMode.Create, FileAccess.Write)
                ms.WriteTo(file)
                file.Close()
                ms.Close()

                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function

    Public Function writeToFile(ByVal pFilename As String, ByVal pFilepath As String, pMemStream As MemoryStream) As Boolean

        ' url: http://net-informations.com/q/faq/memory.html

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                'Dim ms As New MemoryStream(cRawData)
                Dim file As New FileStream(pFilepath + pFilename, FileMode.Create, FileAccess.Write)
                pMemStream.WriteTo(file)
                file.Close()
                pMemStream.Close()

                Return True
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

        Return False

    End Function


    Public Function readFilePdfFromDb(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer, ByVal pFilePath As String) As Integer

        Dim _SQL As String = <![CDATA[ 
SELECT
  pnd.`filebody` AS raw_data,
  pnd.`filesize` AS raw_size
FROM
  `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @PnId
AND pnd.`pn_doc` = @pPnDoc
AND pnd.`pn_potpis` = @pSignPos;

]]>.Value

        Dim myResult As Integer = readFromDb(pPnId, pPnDoc, pSignPos, _SQL, pFilePath)
        Return myResult

    End Function

    Public Function readFilePdfFromDb(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer) As Byte()

        Dim _SQL As String = <![CDATA[ 
SELECT
  pnd.`filebody` AS raw_data,
  pnd.`filesize` AS raw_size
FROM
  `putninalog_doc_documents` pnd
WHERE pnd.`pn_id` = @PnId
AND pnd.`pn_doc` = @pPnDoc
AND pnd.`pn_potpis` = @pSignPos;

]]>.Value

        Dim myResult As Integer = readFromDb(pPnId, pPnDoc, pSignPos, _SQL)
        Return cRawData

    End Function

    Public Function readPotpisPfxFromDb(ByVal pId As Integer, ByVal pFilePath As String) As Integer

        Dim _SQL As String = <![CDATA[ 
SELECT
  potpis_pfx AS raw_data,
  potpis_pfx_size AS raw_size
FROM
   evd_potpisi
WHERE id = @id;
]]>.Value


        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@PnId", pId)
                mycmd.Prepare()

                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    cRawSize = rd.GetUInt32(rd.GetOrdinal("raw_size"))

                    cRawData = New Byte(cRawSize) {}
                    rd.GetBytes(rd.GetOrdinal("raw_data"), 0, cRawData, 0, cRawSize)

                    fs = New FileStream(pFilePath, FileMode.OpenOrCreate, FileAccess.Write)
                    fs.Write(cRawData, 0, cRawSize)
                    fs.Close()
                End While

                rd.Close()
                myResult = 1
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using


        Return myResult

    End Function

    Public Function readPotpisPfxFromDb(ByVal pId As Integer) As Byte()

        Dim _SQL As String = <![CDATA[ 
SELECT
  potpis_pfx AS raw_data,
  potpis_pfx_size AS raw_size
FROM
   evd_potpisi
WHERE id = @id;
]]>.Value


        Dim mycmd As MySqlCommand


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@id", pId)
                mycmd.Prepare()

                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    cRawSize = rd.GetUInt32(rd.GetOrdinal("raw_size"))

                    cRawData = New Byte(cRawSize) {}
                    rd.GetBytes(rd.GetOrdinal("raw_data"), 0, cRawData, 0, cRawSize)
                End While

                rd.Close()
            Catch ex As Exception
                Console.WriteLine(ex.Message)

            End Try

        End Using


        Return cRawData

    End Function


    Public Function readPotpisImgFromDb(ByVal pId As Integer, ByVal pFilePath As String) As Integer

        Dim _SQL As String = <![CDATA[ 
SELECT
  potpis_img AS raw_data,
  potpis_img_size AS raw_size
FROM
   evd_potpisi
WHERE id = @id;
]]>.Value


        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@PnId", pId)
                mycmd.Prepare()

                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    cRawSize = rd.GetUInt32(rd.GetOrdinal("raw_size"))

                    cRawData = New Byte(cRawSize) {}
                    rd.GetBytes(rd.GetOrdinal("raw_data"), 0, cRawData, 0, cRawSize)

                    fs = New FileStream(pFilePath, FileMode.OpenOrCreate, FileAccess.Write)
                    fs.Write(cRawData, 0, cRawSize)
                    fs.Close()
                End While

                rd.Close()
                myResult = 1
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using


        Return myResult

    End Function

    Public Function readPotpisImgFromDb(ByVal pId As Integer) As Byte()

        Dim _SQL As String = <![CDATA[ 
SELECT
  potpis_img AS raw_data,
  potpis_img_size AS raw_size
FROM
   evd_potpisi
WHERE id = @id;
]]>.Value

        Dim mycmd As MySqlCommand


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = _SQL
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@id", pId)
                mycmd.Prepare()

                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    cRawSize = rd.GetUInt32(rd.GetOrdinal("raw_size"))

                    cRawData = New Byte(cRawSize) {}
                    rd.GetBytes(rd.GetOrdinal("raw_data"), 0, cRawData, 0, cRawSize)
                End While

                rd.Close()
            Catch ex As Exception
                Console.WriteLine(ex.Message)

            End Try

        End Using


        Return cRawData

    End Function


    Public Function readFromDb(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer, ByVal pSql As String, ByVal pFilePath As String) As Integer

        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = pSql
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@PnId", pPnId)
                mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
                mycmd.Parameters.AddWithValue("@PpSignPos", pSignPos)
                mycmd.Prepare()

                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    cRawSize = rd.GetUInt32(rd.GetOrdinal("raw_size"))

                    cRawData = New Byte(cRawSize) {}
                    rd.GetBytes(rd.GetOrdinal("raw_data"), 0, cRawData, 0, cRawSize)

                    fs = New FileStream(pFilePath, FileMode.OpenOrCreate, FileAccess.Write)
                    fs.Write(cRawData, 0, cRawSize)
                    fs.Close()
                End While

                rd.Close()
                myResult = 1
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using

        Return myResult

    End Function

    Public Function readFromDb(ByVal pPnId As Integer, ByVal pPnDoc As Integer, ByVal pSignPos As Integer, ByVal pSql As String) As Integer

        Dim mycmd As MySqlCommand
        Dim myResult As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            Try
                myconnection.Open()

                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                mycmd.CommandText = pSql
    

                mycmd.Parameters.Clear()
                mycmd.Parameters.AddWithValue("@PnId", pPnId)
                mycmd.Parameters.AddWithValue("@pPnDoc", pPnDoc)
                mycmd.Parameters.AddWithValue("@PpSignPos", pSignPos)
                mycmd.Prepare()

                Dim rd As MySqlDataReader = mycmd.ExecuteReader()

                While rd.Read
                    cRawSize = rd.GetUInt32(rd.GetOrdinal("raw_size"))

                    cRawData = New Byte(cRawSize) {}
                    rd.GetBytes(rd.GetOrdinal("raw_data"), 0, cRawData, 0, cRawSize)

                End While

                rd.Close()
                myResult = 1
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                myResult = -2
            End Try

        End Using

        Return myResult

    End Function

    Public Function getRandomFileName(ByVal pPutNalId As Integer, ByVal pPutNalDoc As Integer) As String

        Dim _randFilename As String = Path.GetRandomFileName()

        _randFilename = _randFilename.Replace(".", "")
        _randFilename = String.Format("putnal_{0}_{1}_{2}.pdf", pPutNalId.ToString, pPutNalDoc.ToString, _randFilename)

        Return _randFilename

    End Function


End Class
