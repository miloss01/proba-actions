import re

SCOPE_RE = re.compile(r'^(?P<type>repository|registry):(?P<name>[^:]+)(?::(?P<tag>[^:]))?:(?P<actions>.*)$')

class Scope(object):
    def __init__(self, scope_type, name, tag, actions):
        self.type = scope_type
        self.name = name
        self.tag = tag
        self.actions = actions

    def __repr__(self):
        return f"Scope({self.type!r}, {self.name!r}, {self.tag!r}, {self.actions!r})"

    @classmethod
    def parse(cls, scope_str):
        if isinstance(scope_str, bytes):
            scope_str = scope_str.decode('utf8')
        parsed = SCOPE_RE.match(scope_str.strip().lower())
        return Scope(parsed.group('type'),
                     parsed.group('name'),
                     parsed.group('tag'),
                     parsed.group('actions').split(','))