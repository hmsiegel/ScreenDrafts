# Domain Models

##   Draft
```JSON
{
	"id": "f936f335-9b94-43d4-aa45-65e283561aca",
	"name": "70's Conspiracy Thrillers",
	"draftType": "regular",
	"releaseDate" :"2020-01-01T00:00:00.0000000Z", 
	"runtime" : "201 minutes",
	"episodeNumber": "225",
    "numberOfDrafters": 2,
    "drafterIds": [
		 "00000000-0000-0000-0000-000000000000",
    ],
	"hostIds": [
		 "00000000-0000-0000-0000-000000000000",
		 "00000000-0000-0000-0000-000000000000"
	],
	"selectedMovies": [
		{
			"selectedMovieId": "00000000-0000-0000-0000-000000000000",
			"movieId": "00000000-0000-0000-0000-000000000000 (The Mist)",
			"draftPosition": 7,
			"gmId": "00000000-0000-0000-0000-000000000000 (Billy Ray Brewton)",
			"vetoed" : "false",
			"vetoedById": "",
			"vetoOverride": "N/A",
			"vetoOverrideId": ""
		}
	]
    "createdDateTime": "2020-01-01T00:00:00.0000000Z",
    "updatedDateTime": "2020-01-01T00:00:00.0000000Z"
}
```

```csharp
class Draft
{
	Draft Create();
	void AddDraftedMovie(Movie movie, Guid gmId);
	void AddGeneralManagers(Guid gmId);
	void UpdateDraft(Draft draftDetails);
	void RemoveDraft(Guid draftId);
	void AddVetoedMovie(Movie movie, Guid gmId);
	void AddVetoOverrideMovie(Movie movie, Guid gmId);
	void AddComissionerOverrideMovie(Movie movie, Guid gmId);
}
```

