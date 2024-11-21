# AWS Setup

This project includes a PowerShell script (`aws-setup.ps1`) and a configuration file (`configuration.yaml`) to automate the setup of AWS resources.

## Configuration File

The `configuration.yaml` file contains the necessary configuration for setting up IAM users and secrets in AWS Secrets Manager. Below is a brief description of the fields:

- `iam`:
  - `username`: The name of the IAM user to be created.
  - `policy`: The name of the IAM policy to be created and attached to the user.
- `secrets`:
  - `prefix`: The prefix for the secrets in AWS Secrets Manager.
  - `region`: The AWS region where the secrets will be stored.
  - `hashing-key`, `orchestrator-client-secret`, `orchestrator-client-name`, `microsoft-client-with-teams-id`, `microsoft-client-with-teams-secret`, `slack-client-id`, `slack-client-secret`, `jira-client-id`, `jira-client-secret`: Placeholder values for the secrets to be created.

## PowerShell Script

The `aws-setup.ps1` script reads the configuration from `configuration.yaml` and performs the following tasks:

1. Installs necessary PowerShell modules.
2. Reads and parses the YAML configuration file.
3. Creates or updates secrets in AWS Secrets Manager.
4. Creates an IAM user and attaches the specified policy.
5. Creates the IAM policy with permissions to manage the secrets.

### Usage

1. Ensure you have the necessary AWS CLI and PowerShell modules installed.
2. Update the `configuration.yaml` file with your specific values.
3. Run the `aws-setup.ps1` script: