using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace EDF
{
    public class EDFLocalPatientIdentification
    {
        public EDFLocalPatientIdentification()
        {
            //parameterless constructor, required for XML serialization
        }
        public EDFLocalPatientIdentification(char[] patientIdentification)
        {
            parsePatientIdentificationSubFields(patientIdentification);
        }
        public EDFLocalPatientIdentification(string patientCode, string patientSex, DateTime patientBirthDate, string patientName, List<string> patientAdditional)
        {
            setupPatient(patientCode, patientSex, patientBirthDate, patientName, patientAdditional);
        }
        public EDFLocalPatientIdentification(string patientCode, string patientSex, DateTime patientBirthDate, string patientName)
        {
            setupPatient(patientCode, patientSex, patientBirthDate, patientName, new List<string>());
        }
        private void setupPatient(string patientCode, string patientSex, DateTime patientBirthDate, string patientName, List<string> patientAdditional)
        {
            this.PatientCode = patientCode;
            this.PatientSex = patientSex;
            this.PatientBirthDate = patientBirthDate;
            this.PatientName = patientName;
            this.AdditionalPatientIdentification = patientAdditional;
        }
        public string PatientCode { get; set; }
        public string PatientSex { get; set; }
        public DateTime PatientBirthDate { get; set; }
        public string PatientName { get; set; }
        public List<string> AdditionalPatientIdentification { get; set; }
        
        private void parsePatientIdentificationSubFields(char[] patientIdentification)
        {
            //_strPatientIdentification = null;//by zt 
            StringBuilder _myPatientIdentification;
            _myPatientIdentification = null;
            _myPatientIdentification = new StringBuilder(new string(patientIdentification));
            string[] arrayPatientInformation = _myPatientIdentification.ToString().Trim().Split(' ');
            this.AdditionalPatientIdentification = new List<string>();

            if (arrayPatientInformation.Length >= 4)
            {
                this.PatientCode = arrayPatientInformation[0];
                this.PatientSex = arrayPatientInformation[1];
                try
                {
                    this.PatientBirthDate = DateTime.Parse(arrayPatientInformation[2]);
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine("A FormatException occurred on the Patient BirthDate, this is not in EDF+ format\n\n" + ex.StackTrace);
                    this.intializeEDF();
                    return;
                }
                this.PatientName = arrayPatientInformation[3];

                
                for (int i = 4; i < arrayPatientInformation.Length; i++)
                {
                    AdditionalPatientIdentification.Add(arrayPatientInformation[i]);
                }
            }
            else
            {
                this.intializeEDF();
            }
        }
        private void intializeEDF()
        {
            this.PatientCode = string.Empty;
            this.PatientSex = string.Empty;
            this.PatientBirthDate = DateTime.MinValue;
            this.PatientName = string.Empty;
        }
        /// <summary>
        /// 重载ToString方法  --by zt
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            StringBuilder _strPatientIdentificationToString = new StringBuilder(String.Empty);
            _strPatientIdentificationToString = new StringBuilder(string.Empty);
            _strPatientIdentificationToString.Append(PatientCode);
            _strPatientIdentificationToString.Append(" ");
            _strPatientIdentificationToString.Append(PatientSex);
            _strPatientIdentificationToString.Append(" ");
            if (!PatientBirthDate.Equals(DateTime.MinValue))
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                _strPatientIdentificationToString.Append(PatientBirthDate.ToString("dd-MMM-yyyy", culture));
            }
            _strPatientIdentificationToString.Append(" ");
            _strPatientIdentificationToString.Append(PatientName);
            foreach (string info in AdditionalPatientIdentification)
            {
                _strPatientIdentificationToString.Append(" ");
                _strPatientIdentificationToString.Append(info);
            }
            _strPatientIdentificationToString = new StringBuilder(_strPatientIdentificationToString.Length > EDFHeader.FixedLength_LocalPatientIdentification ? _strPatientIdentificationToString.ToString().Substring(0, EDFHeader.FixedLength_LocalPatientIdentification) : _strPatientIdentificationToString.ToString().PadRight(EDFHeader.FixedLength_LocalPatientIdentification));
            return _strPatientIdentificationToString.ToString();

        }
    }
}
