# All Aboard! 

Micro-services are hard, messaging is hard, without implementing some complex patterns to ensure that you do not loose consistency.

ensure you test your use-cases


## Features 

* Atomic message sending - ensure our database is correct 
* Message Filter - process a message only once
* Meta data - keep your correlation Id and other meta data
* Use your message library (i.e. MassTransit)
* Use your datastore library (i.e. Marten)

# Example

what could go wrong, here is a simple message consumer

```
public class BasketCheckedOutConsumer : IConsumer<Basket.Events.BasketCheckedOut>
{
    private readonly IDocumentSession _documentSession;
    private readonly IBus _bus;

    public BasketCheckedOutConsumer(IDocumentSession documentSession, IBus bus)
    {
        _documentSession = documentSession;
        _bus = bus;
    }
    
    
    public async Task Consume(ConsumeContext<BasketCheckedOut> context)
    {
        //POINT A <== here
        var basket = context.Message.Basket;

        var reserved = new ReservedStock()
        {
            Id = basket.Id
        };
        //do more logic

        //save the reserved entry
        _documentSession.Insert(reserved);
        
        
        var reservedStock = new StockReserved()
        {
            Id = reserved.Id,
            Items = reserved.Items
        };

        //POINT B <== here
        await _bus.Publish(reservedStock);
        await _documentSession.SaveChanges(); 
    }
}

```
only 2 main points but what is the repercussion.

lets go over them

- A > duplicate messages

we have not defence logic to ensure that we have not already processed the message (most bus's ensure at least once delivery, meaning your service may process the message multiple times)

- B > publishing messages separately from the database transaction

we send a message onto the bus, then we save our state, looks ok, but what happens if the transaction fails.... we end up providing false messages on the bus an causing inconsistent state across multiple if not all services.


## how do we fix this??

To be honest the above code will look the same! mostly, but we will implement a couple of tools which AllAboard provides

for the code above we need to change the IBus, and use the one from AllAboard.

and then we need to register AllAboard.

```
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)

        //HERE
        .ConfigureAllAboard(setup =>
        {
            //this sets up AllAboard to use Martin and Masstransit
            setup.UseDataStore<Marten>();
            setup.UseMessaging<MassTransit>();
        })

        .ConfigureServices(services =>
        {
            //SETUP Messaging
            services.AddMassTransit(config =>
            {
                config.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    //add all the normal setup for this component
                    cfg.UseServiceScope(provider);
                    
                    //HERE - register AllAboard middleware
                    cfg.UseAllAboard(); 
                }));


            });

            //SET UP Database
            services.AddSingleton<IDocumentStore>(s =>
            {
                //setup as normal
            });

            //the db session needs to be set to scoped.
            services.AddScoped(s => s.GetService<IDocumentStore>().DirtyTrackedSession());
        });
```

This example we need to make 2 changes to the host setup

1. add ConfigureAllAboard directly to the IHostBuilder
2. add UseAllAboard to MassTransit

note some extensions may be setup differently, but in this example we are done

# How does that work?

AllAboard implements middleware and its own IBus along with a background processor

## Middleware

All the consumers will have middleware which filters out messages which have been processed more than once

It stores minimal information in the applications database to know which messages have been processed

## IBus

The IBus provided by AllAboard will look like and acts like the normally bus.publish. however it utilise the current ISession to save the message in the database for background processing. 

This ensures that we do not loose a message, and also that we do not accidentally publish one without saving state.

## Background processor

This has the logic to take the messages we wanted to publish during processing messages. As the applications state is correct this component ensures the messages are published (at least once)

The background process should be triggered after any request/consume

## Extension

It is easy to add middleware to ASP.NET to ensure to trigger the backroundworker




