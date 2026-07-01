module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      [
        'feat',
        'fix',
        'refactor',
        'perf',
        'style',
        'test',
        'docs',
        'chore',
        'i18n',
        'ci',
        'build',
        'revert'
      ]
    ],
    'scope-enum': [
      2,
      'always',
      [
        'commands',
        'viewport',
        'palettes',
        'geometry',
        'database',
        'io',
        'themes',
        'ui',
        'tests',
        'build',
        'i18n',
        'release'
      ]
    ]
  }
};
