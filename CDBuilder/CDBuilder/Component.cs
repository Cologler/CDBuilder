namespace CDBuilder
{
    class Component
    {
        public Component(IComponent instance, CommandAttribute command)
        {
            this.Instance = instance;
            this.Command = command;
        }

        public IComponent Instance { get; private set; }

        public CommandAttribute Command { get; private set; }
    }
}