/**
 * Simple C# wrapper for Google Analytics Measurement Protocol.
 * 
 * See https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide for more details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Google Analytics Measurement Protocol
/// </summary>
public class GAMP {
    /// <summary>
    /// Send a single hit to GA.
    /// </summary>
    /// <param name="options">Values to send.</param>
    public static void PostSingle(Options options) {
        PostToGA(new List<Options> {options});
    }

    /// <summary>
    /// Send a single hit, analyzed from request, to GA.
    /// </summary>
    public static void PostSingle(HttpRequest request, Options options) {
        var rnd = new Random();
        var cid = rnd.Next(1, int.MaxValue);

        var op = new Options {
            AnonymousCliendID = cid.ToString(),
            Page = request.Path.ToString(),
            UserIP = request.HttpContext.Connection.RemoteIpAddress.ToString(),
            UserAgent = request.Headers["User-Agent"].ToString()
        };

        if (!string.IsNullOrWhiteSpace(options.Version)) {
            op.Version = options.Version;
        }

        if (!string.IsNullOrWhiteSpace(options.TrackingID)) {
            op.TrackingID = options.TrackingID;
        }

        if (!string.IsNullOrWhiteSpace(options.AnonymousCliendID)) {
            op.AnonymousCliendID = options.AnonymousCliendID;
        }

        if (!string.IsNullOrWhiteSpace(options.HitType)) {
            op.HitType = options.HitType;
        }

        if (!string.IsNullOrWhiteSpace(options.Page)) {
            op.Page = options.Page;
        }

        if (!string.IsNullOrWhiteSpace(options.UserIP)) {
            op.UserIP = options.UserIP;
        }

        if (!string.IsNullOrWhiteSpace(options.UserAgent)) {
            op.UserAgent = options.UserAgent;
        }

        if (options.CustomValues != null &&
            options.CustomValues.Any()) {
            op.CustomValues = new Dictionary<string, string>();

            foreach (var item in options.CustomValues) {
                op.CustomValues.Add(item.Key, item.Value);
            }
        }

        PostSingle(op);
    }

    /// <summary>
    /// Send multiple hits to GA.
    /// </summary>
    /// <param name="options">List of values to send.</param>
    public static void PostBatch(List<Options> options) {
        PostToGA(options);
    }

    /// <summary>
    /// Do the actual GA transmission.
    /// </summary>
    /// <param name="options">List of values to send.</param>
    private static void PostToGA(IReadOnlyCollection<Options> options) {
        var values = new List<string>();

        foreach (var opt in options) {
            var dict = new Dictionary<string, string>();

            // Version
            if (string.IsNullOrWhiteSpace(opt.Version)) {
                throw new MissingMemberException("Version (v) is required.");
            }

            dict.Add("v", opt.Version);

            // TrackingID ID
            if (string.IsNullOrWhiteSpace(opt.TrackingID)) {
                throw new MissingMemberException("TrackingID (tid) is required.");
            }

            dict.Add("tid", opt.TrackingID);

            // AnonymousCliendID
            if (string.IsNullOrWhiteSpace(opt.AnonymousCliendID)) {
                throw new MissingMemberException("AnonymousCliendID (cid) is required.");
            }

            dict.Add("cid", opt.AnonymousCliendID);

            // HitType
            if (string.IsNullOrWhiteSpace(opt.HitType)) {
                throw new MissingMemberException("HitType (t) is required.");
            }

            dict.Add("t", opt.HitType);

            // Page
            if (!string.IsNullOrWhiteSpace(opt.Page)) {
                dict.Add("dp", opt.Page);
            }

            // UserIP
            if (!string.IsNullOrWhiteSpace(opt.UserIP)) {
                dict.Add("uip", opt.UserIP);
            }

            // UserAgent
            if (!string.IsNullOrWhiteSpace(opt.UserAgent)) {
                dict.Add("ua", opt.UserAgent);
            }

            // CustomValues
            if (opt.CustomValues != null &&
                opt.CustomValues.Any()) {
                foreach (var item in opt.CustomValues) {
                    dict.Add(item.Key, item.Value);
                }
            }

            // Cycle and add to values.
            values.Add(dict.Aggregate("", (c, p) => c + "&" + p.Key + "=" + WebUtility.UrlEncode(p.Value)).Substring(1));
        }

        var url = options.Count == 1 ? "collect" : "batch";
        var client = new HttpClient();
        var content = new StringContent(string.Join("\n", values), Encoding.UTF8);

        client.PostAsync(
            "https://www.google-analytics.com/" + url,
            content);
    }

    /// <summary>
    /// Values to send to GA.
    /// </summary>
    public class Options {
        /// <summary>
        /// Version. (required)
        /// </summary>
        public string Version = "1";

        /// <summary>
        /// Tracking/Property ID. (required)
        /// </summary>
        public string TrackingID { get; set; }

        /// <summary>
        /// Anonymous Client ID. (required)
        /// </summary>
        public string AnonymousCliendID { get; set; }

        /// <summary>
        /// Hit Type. (required)
        /// </summary>
        public string HitType = "pageview";

        /// <summary>
        /// Page.
        /// </summary>
        public string Page { get; set; }

        /// <summary>
        /// IP address override.
        /// </summary>
        public string UserIP { get; set; }

        /// <summary>
        /// User agent override.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Custom values to include.
        /// </summary>
        public Dictionary<string, string> CustomValues { get; set; }
    }
}