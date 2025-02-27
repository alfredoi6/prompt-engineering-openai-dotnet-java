namespace NunitTests.Models;


public class InvoiceEntryList
{
	public List<InvoiceEntryOutput> InvoiceEntries { get; set; }
}

public class InvoiceEntryOutput
{
	public string OriginalCommitAuthor { get; set; }
	public string OriginalCommitMessage { get; set; }
	public string SummaryOfTheWorkDone { get; set; }
	public decimal EstimatedTimeSpentOnTask { get; set; }
	public string JustificationForWhyTheWorkWasNecessary { get; set; }
}