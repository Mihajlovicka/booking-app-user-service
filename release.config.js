module.exports = {
    branches: [
        { name: 'master' },                     // Default main branch for releases
        { name: 'develop', channel: 'next' }, // Pre-releases from 'develop'
        { name: 'feature-*', prerelease: true }, // Prerelease branches for feature branches
      ],
    plugins: [
      '@semantic-release/commit-analyzer',
      '@semantic-release/release-notes-generator',
      '@semantic-release/github',
      '@semantic-release/git',
      '@semantic-release/exec',
    ],
  };
  