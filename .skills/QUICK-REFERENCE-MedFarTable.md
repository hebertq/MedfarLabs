# QUICK REFERENCE - MedFarTable

## Quick-Start Examples

```javascript
// Example 1: Basic Usage
<MedFarTable data={data} />

// Example 2: Customizing Columns
<MedFarTable data={data} columns={customColumns} />
Component Signatures
MedFarTable
Props:
data: Array of data objects to be displayed.
columns: Optional array to define custom columns.
Example Props Structure
JavaScript
const data = [
  { id: 1, name: 'Patient A', status: 'active' },
  { id: 2, name: 'Patient B', status: 'inactive' }
];

const customColumns = [
  { title: 'Name', dataIndex: 'name' },
  { title: 'Status', dataIndex: 'status' }
];
Icon Mappings
Context	Icon
Patients	patient_icon.png
Samples	sample_icon.png
Invoices	invoice_icon.png
Appointments	appointment_icon.png
Medicines	medicine_icon.png
Copy-Paste Templates
Patient Entry
JavaScript
const newPatient = {
  id: 3,
  name: 'New Patient',
  status: 'active'
};
Invoice Entry
JavaScript
const newInvoice = {
  id: 1,
  amount: 100,
  status: 'paid'
};
Medicine Entry
JavaScript
const newMedicine = {
  id: 1,
  name: 'Medicine A',
  dose: '500mg'
};
Code
