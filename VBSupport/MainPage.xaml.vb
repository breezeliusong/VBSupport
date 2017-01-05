' The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

Imports Windows.Storage.Streams
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
    ' This is the click handler for the 'Copy Strings' button.  Here we will parse the
    ' strings contained in the ElementsToWrite text block, write them to a stream using
    ' DataWriter, retrieve them using DataReader, and output the results in the
    ' ElementsRead text block.
    Private Async Sub TransferData(ByVal sender As Object, ByVal e As RoutedEventArgs)
        ' Initialize the in-memory stream where data will be stored.
        Dim stream = New Windows.Storage.Streams.InMemoryRandomAccessStream
        ' Create the data writer object backed by the in-memory stream.
        Dim dataWriter = New Windows.Storage.Streams.DataWriter(stream)
        dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8
        dataWriter.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian
        ' Parse the input stream and write each element separately.
        Dim text As String = "Hello;world"
        Dim inputElements() As String = text.Split(Microsoft.VisualBasic.ChrW(59))
        For Each inputElement As String In inputElements
            Dim inputElementSize As UInteger = dataWriter.MeasureString(inputElement)
            dataWriter.WriteUInt32(inputElementSize)
            dataWriter.WriteString(inputElement)
        Next
        ' Send the contents of the writer to the backing stream.
        Await dataWriter.StoreAsync()
        ' For the in-memory stream implementation we are using, the flushAsync call 
        ' is superfluous,but other types of streams may require it.
        Await dataWriter.FlushAsync()
        ' In order to prolong the lifetime of the stream, detach it from the 
        ' DataWriter so that it will not be closed when Dispose() is called on 
        ' dataWriter. Were we to fail to detach the stream, the call to 
        ' dataWriter.Dispose() would close the underlying stream, preventing 
        ' its subsequent use by the DataReader below.
        dataWriter.DetachStream()
        ' Create the input stream at position 0 so that the stream can be read 
        ' from the beginning.
        Dim inputStream = stream.GetInputStreamAt(0)
        Dim dataReader = New Windows.Storage.Streams.DataReader(inputStream)
        ' The encoding and byte order need to match the settings of the writer 
        ' we previously used.
        dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8
        dataReader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian
        ' Once we have written the contents successfully we load the stream.
        Await dataReader.LoadAsync(CType(stream.Size, UInteger))
        Dim receivedStrings = ""
        ' Keep reading until we consume the complete stream.

        Dim buffer As IBuffer = dataReader.DetachBuffer()
        Dim array As Byte() = buffer.ToArray

        Dim str As IOutputStream = New InMemoryRandomAccessStream()
        Dim recv1 As DataReader = New DataReader(str)
        'recv1 = DataReader.FromBuffer(buffer)
        Await recv1.LoadAsync(buffer.Length)

        Dim ch As Byte
        ch = recv1.ReadByte()

        While (dataReader.UnconsumedBufferLength > 0)
            ' Note that the call to readString requires a length of "code units" 
            ' to read. This is the reason each string is preceded by its length 
            ' when "on the wire".
            Dim bytesToRead As UInteger = dataReader.ReadUInt32
            receivedStrings = (receivedStrings _
                        + (dataReader.ReadString(bytesToRead) + "" & vbLf))
        End While
        ' Populate the ElementsRead text block with the items we read 
        ' from the stream.
    End Sub
End Class
