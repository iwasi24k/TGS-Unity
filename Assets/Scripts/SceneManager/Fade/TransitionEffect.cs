using System.Collections;

public abstract class TransitionEffect
{
    public abstract IEnumerator PlayOut();

    public abstract IEnumerator PlayIn();
}