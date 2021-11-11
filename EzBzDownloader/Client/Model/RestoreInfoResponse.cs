using System.Xml.Serialization;
using JetBrains.Annotations;

namespace EzBzDownloader.Client.Model
{
    [XmlRoot(ElementName = "content")]
    public class Content
    {
        [XmlElement(ElementName = "response")]
        public Response Response { get; set; }

        [XmlElement(ElementName = "restore")]
        public Restore[] Restores { get; set; }
    }

    [XmlRoot(ElementName = "response")]
    public class Response
    {
        [XmlAttribute(AttributeName = "result")]
        public bool Result { get; set; }

        [XmlAttribute(AttributeName = "reason")]
        public string Reason { get; set; }
    }

    [XmlRoot(ElementName = "restore")]
    public class Restore
    {
        [XmlElement(ElementName = "billing")]
        [CanBeNull]
        public string Billing { get; set; }

        [XmlElement(ElementName = "shipping")]
        [CanBeNull]
        public string Shipping { get; set; }

        [XmlAttribute(AttributeName = "date")]
        public long Date { get; set; }

        [XmlAttribute(AttributeName = "missing_chunk")]
        public bool MissingChunk { get; set; }

        [XmlAttribute(AttributeName = "predicted_restore_time")]
        public int PredictedRestoreTime { get; set; }

        [XmlAttribute(AttributeName = "finished_date_hr")]
        public long FinishedDateHr { get; set; }

        [XmlAttribute(AttributeName = "finished_date")]
        public long FinishedDate { get; set; }

        [XmlAttribute(AttributeName = "rid")]
        public string Rid { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "locale")]
        public string Locale { get; set; }

        [XmlAttribute(AttributeName = "dest")]
        public string Dest { get; set; }

        [XmlAttribute(AttributeName = "processed_time_sec")]
        public int ProcessedTimeSec { get; set; }

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }

        [XmlAttribute(AttributeName = "enc_pass_key")]
        public string EncPassKey { get; set; }

        [XmlAttribute(AttributeName = "last_backup_time")]
        public string LastBackupTime { get; set; }

        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }

        [XmlAttribute(AttributeName = "serverhost")]
        public string Serverhost { get; set; }

        [XmlAttribute(AttributeName = "tracking_number")]
        public string TrackingNumber { get; set; }

        [XmlAttribute(AttributeName = "state")]
        public string State { get; set; }

        [XmlAttribute(AttributeName = "hguid")]
        public string Hguid { get; set; }

        [XmlAttribute(AttributeName = "num_of_files_copied")]
        public int NumOfFilesCopied { get; set; }

        [XmlAttribute(AttributeName = "has_host_key")]
        public bool HasHostKey { get; set; }

        [XmlAttribute(AttributeName = "interrupt_reason")]
        public string InterruptReason { get; set; }

        [XmlAttribute(AttributeName = "file_transfer_time_sec")]
        public int FileTransferTimeSec { get; set; }

        [XmlAttribute(AttributeName = "zipfile")]
        public string Zipfile { get; set; }

        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }

        [XmlAttribute(AttributeName = "os")]
        public string Os { get; set; }

        [XmlAttribute(AttributeName = "is_mobile")]
        public bool IsMobile { get; set; }

        [XmlAttribute(AttributeName = "submitted_by_group_admin_account_id")]
        public string SubmittedByGroupAdminAccountId { get; set; }

        [XmlAttribute(AttributeName = "num_of_files")]
        public int NumOfFiles { get; set; }

        [XmlAttribute(AttributeName = "display")]
        public bool Display { get; set; }

        [XmlAttribute(AttributeName = "zipsize")]
        public long Zipsize { get; set; }

        [XmlAttribute(AttributeName = "is_save_to_b2")]
        public bool IsSaveToB2 { get; set; }

        [XmlAttribute(AttributeName = "user_ip_addr")]
        public string UserIpAddr { get; set; }

        [XmlAttribute(AttributeName = "top_level_drive_name")]
        public string TopLevelDriveName { get; set; }

        [XmlAttribute(AttributeName = "baseURL")]
        public string BaseURL { get; set; }

        [XmlAttribute(AttributeName = "carrier")]
        public string Carrier { get; set; }

        [XmlAttribute(AttributeName = "account_id")]
        public string AccountId { get; set; }

        [XmlAttribute(AttributeName = "display_filename")]
        public string DisplayFilename { get; set; }

        [XmlAttribute(AttributeName = "selected_size")]
        public long SelectedSize { get; set; }

        [XmlAttribute(AttributeName = "size")]
        public long Size { get; set; }

        [XmlAttribute(AttributeName = "restore_in_progress")]
        public bool RestoreInProgress { get; set; }

        [XmlAttribute(AttributeName = "cluster_num")]
        public int ClusterNum { get; set; }

        [XmlAttribute(AttributeName = "date_hr")]
        public long DateHr { get; set; }

        [XmlAttribute(AttributeName = "utf8pathcount")]
        public int Utf8PathCount { get; set; }

        [XmlAttribute(AttributeName = "last_update")]
        public long LastUpdate { get; set; }

        [XmlAttribute(AttributeName = "bz_auth_token")]
        public string BzAuthToken { get; set; }
    }
}