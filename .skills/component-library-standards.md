# Reusable Components Documentation

This document outlines the reusable components used within our project, including their purposes, parameters, usage examples, and design patterns for Smart/Dumb components.

## Components

### 1. LoadingContainer
- **Purpose**: Displays a loading state while content is being fetched.
- **Parameters**:
  - `isLoading`: Boolean indicating whether loading is in progress.
  - `children`: React nodes to be displayed when not loading.
- **Usage Example**:
  ```jsx
  <LoadingContainer isLoading={loading}>
      <YourContent />
  </LoadingContainer>
2. ErrorAlert
Purpose: Displays an error message to inform users of issues.
Parameters:
message: The error message to display.
onClose: Function to handle closing the alert.
Usage Example:
jsx
<ErrorAlert message="An error occurred!" onClose={handleClose} />
3. SuccessAlert
Purpose: Displays a success message after an action.
Parameters:
message: The success message to display.
onClose: Function to handle closing the alert.
Usage Example:
jsx
<SuccessAlert message="Operation successful!" onClose={handleClose} />
4. ConfirmDialog
Purpose: A dialog for user confirmation before proceeding with an action.
Parameters:
message: The confirmation message.
onConfirm: Function to call on confirmation.
onCancel: Function to call on cancellation.
Usage Example:
jsx
<ConfirmDialog 
    message="Are you sure you want to proceed?" 
    onConfirm={handleConfirm} 
    onCancel={handleCancel} 
/>
Design Patterns for Smart/Dumb Components
Smart Components
Responsible for managing state and behavior.
Integrates with APIs and holds logic.
Dumb Components
Stateless components focused on UI.
Receives data and callbacks through props, renders UI based on them.
Code
