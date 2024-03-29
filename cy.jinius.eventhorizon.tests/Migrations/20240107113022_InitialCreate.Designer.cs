﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using cy.jinius.eventhorizon.tests.Models;

#nullable disable

namespace cy.jinius.eventhorizon.tests.Migrations
{
    [DbContext(typeof(TodoReadDbContext))]
    [Migration("20240107113022_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Cy.Jinius.EventHorizon.Read.AggregateSnapshotEntity<cy.jinius.eventhorizon.tests.Models.Todo>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("AggregateSnapshots");
                });

            modelBuilder.Entity("Cy.Jinius.EventHorizon.Read.AggregateSnapshotEntity<cy.jinius.eventhorizon.tests.Models.Todo>", b =>
                {
                    b.OwnsOne("cy.jinius.eventhorizon.tests.Models.Todo", "Payload", b1 =>
                        {
                            b1.Property<Guid>("AggregateSnapshotEntityId")
                                .HasColumnType("uuid")
                                .HasColumnName("AggregateSnapshotEntity<Todo>Id");

                            b1.Property<DateTime>("Created")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("CreatedBy")
                                .HasColumnType("text");

                            b1.Property<string>("CreatedSource")
                                .HasColumnType("text");

                            b1.Property<string>("Detail")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<Guid>("Id")
                                .HasColumnType("uuid");

                            b1.Property<DateTime>("LastModified")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("LastModifiedBy")
                                .HasColumnType("text");

                            b1.Property<string>("LastModifiedSource")
                                .HasColumnType("text");

                            b1.Property<int>("Version")
                                .HasColumnType("integer");

                            b1.HasKey("AggregateSnapshotEntityId");

                            b1.ToTable("AggregateSnapshots");

                            b1.ToJson("Payload");

                            b1.WithOwner()
                                .HasForeignKey("AggregateSnapshotEntityId");

                            b1.OwnsOne("cy.jinius.eventhorizon.tests.Models.TodoTimestamps", "Timestamps", b2 =>
                                {
                                    b2.Property<Guid>("TodoAggregateSnapshotEntityId")
                                        .HasColumnType("uuid");

                                    b2.Property<DateTime?>("CompletedTime")
                                        .HasColumnType("timestamp with time zone");

                                    b2.Property<DateTime?>("StartedTime")
                                        .HasColumnType("timestamp with time zone");

                                    b2.HasKey("TodoAggregateSnapshotEntityId");

                                    b2.ToTable("AggregateSnapshots");

                                    b2.WithOwner()
                                        .HasForeignKey("TodoAggregateSnapshotEntityId");
                                });

                            b1.Navigation("Timestamps")
                                .IsRequired();
                        });

                    b.Navigation("Payload");
                });
#pragma warning restore 612, 618
        }
    }
}
