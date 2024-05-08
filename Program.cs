using Azure;
using Azure.AI.OpenAI.Assistants;

// OpenAI API
var client = new AssistantsClient("");

// For Azure OpenAI service the model name is the "deployment" name
var assistantCreationOptions = new AssistantCreationOptions("gpt-4-turbo")
{
    Name = "Chat with PDF",
    Instructions = "Extract the information from the uploaded PDF file. ",
    Tools = { new CodeInterpreterToolDefinition() },
};

//var fileName = "../../../Models/fan-manual.pdf";
//var fileUploadResponse = await client.UploadFileAsync(fileName, OpenAIFilePurpose.Assistants);
//assistantCreationOptions.FileIds.Add(fileUploadResponse.Value.Id);
//Console.WriteLine($"Uploaded file {fileUploadResponse.Value.Filename}");

var assistantId = "asst_LFZ4HWeXKIOV0YNzZOY5Sbq6"; //await client.CreateAssistantAsync(assistantCreationOptions);
var threadId = "thread_YFhOqJ5iT88PUaP1if0DQhYh"; // await client.CreateThreadAsync();

Console.WriteLine("Ask a question about the file (empty response to quit):");
var question = Console.ReadLine();

while (!string.IsNullOrWhiteSpace(question))
{
    string? lastMessageId = null;

    await client.CreateMessageAsync(threadId, MessageRole.User, question);
    var run = await client.CreateRunAsync(threadId, new CreateRunOptions(assistantId));
    Response<ThreadRun> runResponse;

    do
    {
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        runResponse = await client.GetRunAsync(threadId, run.Value.Id);
        Console.Write($".");
    } while (runResponse.Value.Status == RunStatus.Queued
            || runResponse.Value.Status == RunStatus.InProgress);

    Console.WriteLine(string.Empty);

    var messageResponse = await client.GetMessagesAsync(threadId, order: ListSortOrder.Ascending, after: lastMessageId);
    var found = false;

    foreach (var message in messageResponse.Value.Data)
    {
        lastMessageId = message.Id;
        foreach (var content in message.ContentItems)
        {
            if (content is MessageTextContent textContent)
            {
                if (textContent.Text == question)
                {
                    found = true;
                }

                if (found && textContent.Text != question)
                {
                    found = false;
                    Console.WriteLine(textContent.Text);
                }

            }
        }
    }

    Console.WriteLine("Your response: (leave empty to quit)");
    question = Console.ReadLine();

}

// clean up the file, thread and assistant
Console.WriteLine("Cleaning up and exiting...");
//await client.DeleteFileAsync(fileUploadResponse.Value.Id);
await client.DeleteThreadAsync(threadId);
await client.DeleteAssistantAsync(assistantId);
