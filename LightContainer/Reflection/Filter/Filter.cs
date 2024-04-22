using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LightContainer.Reflection.Filter
{
    public class Filter
    {
        private readonly eFilterAction _action = default;
        private readonly eFilterTarget _target = default;
        private readonly eFilterType _type = default;
        private readonly string _pattern = default;

        public Filter(eFilterAction action, eFilterTarget target, eFilterType type, string pattern)
        {
            _action = action;
            _target = target;
            _type = type;
            _pattern = pattern;
        }
        
        public void Process(IReadOnlyList<Type> original, HashSet<Type> result)
        {
            Func<Type, string> parameter = null;

            switch (_target)
            {
                case eFilterTarget.Assembly:
                    parameter = type => type.Assembly.FullName;
                    break;
                case eFilterTarget.Namespace:
                    parameter = type => type.Namespace;
                    break;
                case eFilterTarget.Type:
                    parameter = type => type.Name;
                    break;
                case eFilterTarget.Full:
                    parameter = type => type.FullName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (_type)
            {
                case eFilterType.StartWith:
                    Apply(result, original.Where(x => !string.IsNullOrEmpty(parameter(x)) && parameter(x).StartsWith(_pattern)));
                    break;
                case eFilterType.EndWith:
                    Apply(result, original.Where(x => !string.IsNullOrEmpty(parameter(x)) && parameter(x).EndsWith(_pattern)));
                    break;
                case eFilterType.Contain:
                    Apply(result, original.Where(x => !string.IsNullOrEmpty(parameter(x)) && parameter(x).Contains(_pattern)));
                    break;
                case eFilterType.Regex:
                    Apply(result, original.Where(x => !string.IsNullOrEmpty(parameter(x)) && Regex.Match(parameter(x), _pattern).Success));
                    break;
                case eFilterType.Equal:
                    Apply(result, original.Where(x => !string.IsNullOrEmpty(parameter(x)) && parameter(x) == (_pattern)));
                    break;
                case eFilterType.Empty:
                    Apply(result, original.Where(x => string.IsNullOrEmpty(parameter(x))));
                    break;
                case eFilterType.All:
                    Apply(result, original);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Apply(HashSet<Type> result, IEnumerable<Type> source)
        {
            switch (_action)
            {
                case eFilterAction.Include:
                    foreach (Type item in source)
                    {
                        if (!result.Contains(item))
                        {
                            result.Add(item);
                        }
                    }

                    break;
                case eFilterAction.Exclude:
                    foreach (Type item in source)
                    {
                        result.Remove(item);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}