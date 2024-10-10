
# ForAChallenge

## Overview
ForAChallenge is a .NET project, currently including an API implementation. This repository is designed for learning, experimentation, or challenges involving backend development.

The overall design of the project is that of an API interface that 
* Pulls specific financial data for a set collection of companies
from the SEC EDGAR database API.
* Aggregates and transforms the data based on a set of criteria and conditions.
* Provides the data in the API response as a trim json payload.

## Project Structure
- `ForaChallenge.Api`: API implementation folder.
- `.gitignore`: Ignored files for version control.
- `ForaChallenge.sln`: Solution file for the project.

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/jasoncastellano/ForAChallenge.git
   ```
2. Open `ForaChallenge.sln` in Visual Studio.
3. Build the solution and run the API.

## Contributions
Feel free to fork and submit pull requests.

## License
Licensed under the MIT License.
