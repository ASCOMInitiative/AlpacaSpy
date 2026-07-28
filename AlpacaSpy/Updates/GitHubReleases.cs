using Octokit;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Updates
{
    public static class GitHubReleases
    {
        public static Task<IReadOnlyList<Release>> GetReleases(string owner, string name)
        {
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentException.ThrowIfNullOrWhiteSpace(owner);

            ArgumentNullException.ThrowIfNull(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            GitHubClient githubClient = new(new ProductHeaderValue($@"{name}-UpdateCheck"));

            return githubClient.Repository.Release.GetAll(owner, name);
        }

        public static Release? LatestRelease(this IEnumerable<Release> releases)
        {
            ArgumentNullException.ThrowIfNull(releases);
            return releases.Where(rp => !rp.Prerelease).Latest();
        }

        public static Release? LatestPrerelease(this IEnumerable<Release> releases)
        {
            ArgumentNullException.ThrowIfNull(releases);
            return releases.Where(rp => rp.Prerelease).Latest();

        }

        public static Release? Latest(this IEnumerable<Release> releases)
        {
            ArgumentNullException.ThrowIfNull(releases);

            Release? latestRelease = null;
            SemVersion? latestVersion = null;

            foreach (Release release in releases)
            {
                SemVersion releaseVersion = release.ReleaseSemVersionFromTag();

                if (latestRelease is null || latestVersion is null || SemVersion.ComparePrecedence(releaseVersion, latestVersion) > 0)
                {
                    latestRelease = release;
                    latestVersion = releaseVersion;
                }
            }

            return latestRelease;
        }

        public static SemVersion ReleaseSemVersionFromTag(this Release release)
        {
            ArgumentNullException.ThrowIfNull(release);
            if (!string.IsNullOrEmpty(release.TagName) && SemVersion.TryParse(release.TagName, SemVersionStyles.AllowV, out SemVersion? _latest_release_version))
            {
                return _latest_release_version;
            }
            return SemVersion.ParsedFrom(0, 0, 0, release.TagName ?? "No Tag");
        }
    }
}
