using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Interactive
{
    public class InputHistory
    {
        private int _index;
        private readonly int _limit;
        private readonly List<string> _commands;
        private string _temp;

        public IEnumerable<string> Commands => _commands;

        public InputHistory(IEnumerable<string> commands, int limit)
        {
            _limit = limit;
            _commands = commands.Take(limit).ToList();
            Current = new StringBuilder();
            Reset();
        }

        public void Add(string input)
        {
            _commands.Add(input);
            while (_commands.Count > _limit)
            {
                _commands.RemoveAt(0);
            }
        }

        public void Reset()
        {
            _index = _commands.Count;
            _temp = "";
            LoadCurrent();
        }

        public void DeleteCurrent()
        {
            if (IndexWithinLimits())
            {
                _commands.RemoveAt(_index);
            }
            else
            {
                _temp = "";
            }
            LoadCurrent();
        }

        public bool HasNext()
        {
            return _commands.Count > 0 && _index < _commands.Count;
        }

        public bool MoveNext()
        {
            if (!HasNext())
            {
                return false;
            }
            SaveCurrent();
            _index++;
            LoadCurrent();
            return true;
        }

        private void LoadCurrent()
        {
            Current.Clear();
            var command = IndexWithinLimits()
                ? _commands[_index]
                : _temp;
            Current.Append(command);
        }

        public bool HasPrevious()
        {
            return _commands.Count > 0 && _index > 0;
        }

        public bool MovePrevious()
        {
            if (!HasPrevious())
            {
                return false;
            }
            SaveCurrent();
            _index--;
            LoadCurrent();
            return true;
        }

        private bool IndexWithinLimits()
        {
            return _commands.Count > 0 && _index >= 0 && _index < _commands.Count;
        }

        private void SaveCurrent()
        {
            if (IndexWithinLimits())
            {
                _commands[_index] = Current.ToString();
            }
            else
            {
                _temp = Current.ToString();
            }
        }

        public StringBuilder Current { get; }
    }
}