using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogSystem
{

    /// <summary>
    ///     Holds the data used to display a requested dialog
    /// </summary>
    public class Conversation
    {
        /// <summary>
        ///     available answers, if <see cref="Type" /> == Single, or otherwise list of requestable dialogs
        /// </summary>
        public readonly ReadOnlyCollection<Answer> Answers;

        /// <summary>
        ///     used by the conversation engine to identify a dialog
        /// </summary>
        public readonly int ID;

        /// <summary>
        ///     User defined dialog tag (if <see cref="Type" /> == Single)
        /// </summary>
        public readonly string Tag;

        /// <summary>
        /// If <see cref="Type"/> == Single, this is the text of the dialog
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// If <see cref="Type"/> == Single, this is the title of the dialog
        /// </summary>
        public readonly string Title;

        /// <summary>
        ///     The type of dialog; if TopicList, it means more than one dialog is available and this conversation lists them in
        ///     <see cref="Answers" />
        /// </summary>
        public readonly ConversationType Type;

        public Conversation(int id, string title, string text, string tag, ConversationType type, List<Answer> answers)
        {
            ID = id;
            Title = title;
            Text = text;
            Tag = tag;
            Answers = answers.AsReadOnly();
            Type = type;
        }

        public class Answer
        {
            public readonly int Index;
            public readonly string Tag;
            public readonly string Text;

            public Answer(int index, string text, string tag)
            {
                Index = index;
                Text = text;
                Tag = tag;
            }
        }
    }
}